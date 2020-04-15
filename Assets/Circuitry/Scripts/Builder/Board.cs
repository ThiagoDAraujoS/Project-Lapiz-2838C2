using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
public class Board {

}

namespace Circuitry.Builder {


    /// <summary>
    /// Builds a game board
    /// </summary>
    public class Builder {
        private enum CornerType { TopLeft = 0, TopRight = 1, BottomLeft = 2, BottomRight = 3 }
        
        /// <summary>
        /// Empty = Not filled yet, Border = Gray area between chips, Perimeter = area around already designated board pieces, Chip = already allocated chip
        /// </summary>
        private enum TileType { Empty = 0, Perimeter = 1, Chip = 2, Border = 3, Connection = 4 }
       
        private struct Corner {
            public Coordinate position;
            public CornerType type;
            public Corner( int x, int y, CornerType type ) {
                position.x = x;
                position.y = y;
                this.type = type;
            }
        }
     
        private TileType[,] tileMap;
        private int SizeX { get => tileMap.GetLength( 0 ); }
        private int SizeY { get => tileMap.GetLength( 1 ); }

        private readonly Dictionary<ChipType, int> chanceList = new Dictionary<ChipType, int>() {
            { ChipType.Bit      , 16 },
            { ChipType.Byte     , 8  },
            { ChipType.Brick    , 8  },
            { ChipType.Square   , 4  },
            { ChipType.Chip     , 4  },
            { ChipType.Memory   , 2  },
            { ChipType.Processor, 1  }
        };

        private readonly int chanceMax;

        private ChipType RandomChipType() {
            int roll     = UnityEngine.Random.Range( 0, chanceMax ),
                maxScore = 0;

            return chanceList.First( ( chance ) => {
                maxScore += chance.Value;
                return roll < maxScore;
            } ).Key;
        }

        public void Initialize( Coordinate size ) {
            tileMap = new TileType[size.x, size.y];
            int x, y;
            for( x = 0; x < size.x; x++ ) {
                tileMap[x, 0] = TileType.Perimeter;
                tileMap[x, size.y - 1] = TileType.Perimeter;
            }
            for( y = 1; y < size.y - 1; y++ ) {
                tileMap[0, y] = TileType.Perimeter;
                tileMap[size.x - 1, y] = TileType.Perimeter;
            }
            for( y = 1; y < size.y - 1; y++ )
                for( x = 1; x < size.x - 1; x++ )
                    tileMap[x, y] = TileType.Empty;
        }
        private bool IsInside( Coordinate coord ) => coord.x >= 0 && coord.y >= 0 && coord.x < SizeX && coord.y < SizeY;
        private bool IsInside( int x, int y ) => x >= 0 && y >= 0 && x < SizeX && y < SizeY;

        public Board Build() {
            //Enqueue first corners
            Queue<Corner> corners = new Queue<Corner>();
            corners.Enqueue( new Corner( 0        , 0        , CornerType.TopLeft       ) );
            corners.Enqueue( new Corner( SizeX - 1, 0        , CornerType.TopRight      ) );
            corners.Enqueue( new Corner( SizeX - 1, SizeY - 1, CornerType.BottomRight   ) );
            corners.Enqueue( new Corner( 0        , SizeY - 1, CornerType.BottomLeft    ) );

            //For as long as there's corners queued
            while( corners.Count > 0 ) {
                //dequeue
                Corner corner = corners.Dequeue();

                //if Coordinate is still a corner
                if( tileMap[corner.position.x, corner.position.y] == TileType.Perimeter ) {

                    //measure what pieces are fit for that position

                    //spawn a fit piece
                    ChipType newChip = ChipType.Chip;
                    bool flipped = false;
                    //print new piece in the tile map
                    Print( ref corner, newChip, ref corners, flipped );
                }
            }
            return new Board();
        }

        private void Print( ref Corner corner, ChipType chip, ref Queue<Corner> corners, bool flipped = false ) {
            //Set the tile as a border tile
            void SetBorder( int x, int y ) {
                if( IsInside( x, y ) )
                    tileMap[x, y] = TileType.Border;
            }
            
            //Set the tile as a perimeter tile and enqueue its corner it needed
            void SetPerimeter( int x, int y, ref Queue<Corner> corners ) {
                if( IsInside( x, y ) && tileMap[x, y] < TileType.Chip )
                    if( tileMap[x, y] == TileType.Perimeter ) {
                        int cornerType = 0;
                        if( !( IsInside( x + 1, y ) && tileMap[x + 1, y] < TileType.Chip ) )
                            cornerType += 1;

                        if( !( IsInside( x, y + 1 ) && tileMap[x, y + 1] < TileType.Chip ) )
                            cornerType += 2;

                        corners.Enqueue( new Corner( x, y, (CornerType)cornerType ) );
                    }
                    else
                        tileMap[x, y] = TileType.Perimeter;
            }

            //Initialize area and pivot points
            Coordinate area  = ( flipped ) ? chip.Size().Flip() : chip.Size(),
                       pivot = corner.position;

            switch( corner.type ) {
                case CornerType.TopRight:
                    pivot.x -= area.x - 1;
                    break;
                case CornerType.BottomRight:
                    pivot.x -= area.x - 1;
                    pivot.y -= area.y - 1;
                    break;
                case CornerType.BottomLeft:
                    pivot.y -= area.y - 1;
                    break;
            }

            //Fill the internal area of the chip with chiptype.chip
            area.Foreach( ( int x, int y ) => tileMap[x + pivot.x, y + pivot.y] = TileType.Chip );

            //Write around the chip border and perimeter data
            int x, y;
            for( x = -1; x <= area.x; x++ ) {
                SetBorder(    pivot.x + x, pivot.y - 1 );
                SetBorder(    pivot.x + x, pivot.y + area.y );
                SetPerimeter( pivot.x + x, pivot.y - 2,          ref corners );
                SetPerimeter( pivot.x + x, pivot.y + area.y + 1, ref corners );
            }
            for( y = -1; y <= area.y; y++ ) {
                SetBorder(    pivot.x - 1,          pivot.y + y );
                SetBorder(    pivot.x + area.x,     pivot.y + y );
                SetPerimeter( pivot.x - 2,          pivot.y + y, ref corners );
                SetPerimeter( pivot.x + area.x + 1, pivot.y + y, ref corners );
            }
        }

        public Builder() => chanceMax = chanceList.Sum( ( chance ) => chance.Value );
    }

    public struct Chip {
        private ChipType type;
        public ChipType Type {
            get => type;
            set {
                if( type != value ) {
                    type = value;
                    Size = type.Size();
                }
            }
        }
        public Coordinate Position { get; set; }
        public Coordinate Size { get; private set; }
        public Chip( ChipType type, Coordinate? position = null ) {
            this.type = type;
            Size = type.Size();
            Position = position ?? Coordinate.zero;
        }
    } 

    public enum ChipType {
        Processor,  // 6x6,
        Memory,     // 9x3
        Chip,       // 6x2,
        Square,     // 3x3,
        Brick,      // 3x2,
        Byte,       // 2x2,
        Bit         // 2x1,
    }

    public static class ChipTypeExt {
        /// <summary>
        /// Get a chip's size as a coordinate
        /// </summary>
        public static Coordinate Size( this ChipType type ) {
            switch( type ) {
                case ChipType.Processor:
                    return new Coordinate( 6, 6 );
                case ChipType.Memory:
                    return new Coordinate( 9, 3 );
                case ChipType.Chip:
                    return new Coordinate( 6, 2 );
                case ChipType.Square:
                    return new Coordinate( 3, 3 );
                case ChipType.Brick:
                    return new Coordinate( 3, 2 );
                case ChipType.Byte:
                    return new Coordinate( 2, 2 );
                case ChipType.Bit:
                    return new Coordinate( 2, 1 );
                default:
                    throw new System.Exception( "ChipType " + type.ToString() + "'s size defined" );
            }
        }
    }
}
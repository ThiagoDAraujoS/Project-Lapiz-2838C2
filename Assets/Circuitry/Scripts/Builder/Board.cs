using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Circuitry.Builder {
    /// <summary>
    /// Builds a game board
    /// </summary>
    public class Builder {
        /* 
         * Place first chip
         * add all the boundary coordinates to the boundary list
         * place 4 (or more) artificial corners
         * 
         * 
         * find corner closest to the center of the board and open it
         * calculate possible pieces for that corner
         * draw random piece for that corner
         * place piece in the corner
         * update boundary list
         * repeated boundaries become corners
         * 
         * board data structure is useful to quickly fetch new boundary tiles
         * tree with boundaries is useful to quickly check for corners
         * corner heap is useful to quickly get the corner closest to the center
         */



        /// <summary>
        /// Chip spawn chance list, change this update how their chance drop
        /// </summary>
        private readonly Dictionary<ChipType, int> chanceList = new Dictionary<ChipType, int>() {
            { ChipType.Processor, 1  },
            { ChipType.Memory   , 2  },
            { ChipType.Chip     , 4  },
            { ChipType.Square   , 4  },
            { ChipType.Brick    , 8  },
            { ChipType.Byte     , 8  },
            { ChipType.Bit      , 16 }
        };

        private readonly int chanceMax;

        private ChipType GenerateRandomChipType() {
            int roll     = UnityEngine.Random.Range( 0, chanceMax ),
                maxScore = 0;

            return chanceList.First( ( chance ) => {
                maxScore += chance.Value;
                return roll < maxScore;
            } ).Key;
        }

        public Builder() {
            chanceMax = chanceList.Sum( (chance) => chance.Value );
        }
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
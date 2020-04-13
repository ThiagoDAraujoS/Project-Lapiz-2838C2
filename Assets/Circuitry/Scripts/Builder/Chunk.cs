using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Circuitry.Builder {
    public class Chunk {
        public Coordinate Size { get; set; }
        public Coordinate Position { get; set; }
        public int ColorId { get; set; }

        public ChipType Type { get; private set; }

        private Chunk[] connections;
        public ref Chunk this[int index] => ref connections[index];

        public Chunk(ChipType type, Coordinate position) {

        }
    }
}


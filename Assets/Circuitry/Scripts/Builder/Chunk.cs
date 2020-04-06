using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Circuitry.Builder
{
    public class Chunk : MonoBehaviour
    {
        public Coordinate Size { get; set; }
        public Coordinate Position { get; set; }
        public List<Chunk> Conections { get; set; }
        public int ColorId { get; set; }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}
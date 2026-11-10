using UnityEngine;

namespace ArrowMaze
{
    public class ArrowSegment : MonoBehaviour
    {
        private Arrow parentArrow;

        public void Initialize(Arrow parent)
        {
            parentArrow = parent;
        }

    }
}

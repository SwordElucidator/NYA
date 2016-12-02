using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Dreamteck.Splines
{
    public class JunctionFollower : MonoBehaviour
    {
        public SplineFollower follower;

        SplineComputer currentComputer;
        List<Node> nodes = new List<Node>();
        List<int> connectionIndices = new List<int>();
        List<int> pointIndices = new List<int>();

        // Use this for initialization
        void Start()
        {
            if (follower == null) follower = GetComponent<SplineFollower>();
            GetAvailableJunctions();
        }

        // Update is called once per frame
        void Update()
        {
           
        }

        /// <summary>
        /// Used to get the available junctions. Call this to update the junction list on the GUI. 
        /// </summary>
        public void GetAvailableJunctions()
        {
            //Get the last SplineComputer in the junction address
            currentComputer = follower.address.elements[follower.address.depth - 1].computer;
            if (currentComputer == null) return;
            //Get the available junctions at the current address
            double startPercent = (double)follower.address.elements[follower.address.depth - 1].startPoint / (currentComputer.pointCount - 1);
            Spline.Direction dir = Spline.Direction.Forward;
            if (follower.address.elements[follower.address.depth - 1].startPoint > follower.address.elements[follower.address.depth - 1].endPoint) dir = Spline.Direction.Backward;
            int[] available = currentComputer.GetAvailableNodeLinksAtPosition(startPercent, dir);
            nodes.Clear();
            connectionIndices.Clear();
            pointIndices.Clear();
            //Make a list of the available junctions which to use in OnGUI for the buttons
            for (int i = 0; i < available.Length; i++)
            {
                Node node = currentComputer.nodeLinks[available[i]].node;
                if (currentComputer.nodeLinks[available[i]].pointIndex == follower.address.elements[follower.address.depth - 1].startPoint) continue;
                Node.Connection[] connections = node.GetConnections();
                for (int n = 0; n < connections.Length; n++)
                {
                    if (connections[n].computer == currentComputer) continue;
                    nodes.Add(node);
                    connectionIndices.Add(n);
                    pointIndices.Add(currentComputer.nodeLinks[available[i]].pointIndex);
                }
            }
        }

        void OnGUI()
        {
            if (follower == null) return;
            float maxWidth = 250f;
            //if the address depth is bigger than 1 (at least one junction is entered), offer an option to exit the last junction
            if(follower.address.depth > 1) {
                if (GUI.Button(new Rect(5, 10, 35, 20), "< X"))
                {
                    follower.ExitAddress(1);
                    GetAvailableJunctions();
                }
            }
            for (int i = 0; i < nodes.Count; i++)
            {
                Node.Connection connection = nodes[i].GetConnections()[connectionIndices[i]];
                GUI.Label(new Rect(0, 30 + 20 * i, maxWidth * 0.75f, 20), connection.computer.name + " [at P" + pointIndices[i] + "]");
                float x = maxWidth - 70f;
                if (connection.pointIndex > 0)
                {
                    if (GUI.Button(new Rect(x, 30 + 20 * i, 35, 20), "◄─"))
                    {
                        follower.EnterAddress(nodes[i], connectionIndices[i], Spline.Direction.Backward);
                        GetAvailableJunctions();
                        break;
                    }
                    x += 35f;
                }
                if (connection.pointIndex < connection.computer.pointCount - 1)
                {
                    if (GUI.Button(new Rect(x, 30 + 20 * i, 35, 20), "─►"))
                    {
                        follower.EnterAddress(nodes[i], connectionIndices[i], Spline.Direction.Forward);
                        GetAvailableJunctions();
                        break;
                    }
                }
            }
        }
    }
}

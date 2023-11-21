// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CullArea.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities,
// </copyright>
// <summary>
//  Represents the cull area used for network culling.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    ///     Represents the cull area used for network culling.
    /// </summary>
    public class CullArea : MonoBehaviour
    {
        private const int MAX_NUMBER_OF_ALLOWED_CELLS = 250;

        public const int MAX_NUMBER_OF_SUBDIVISIONS = 3;

        public Vector2 Center;
        public Vector2 Size = new(25.0f, 25.0f);

        public Vector2[] Subdivisions = new Vector2[MAX_NUMBER_OF_SUBDIVISIONS];

        public int NumberOfSubdivisions;

        public bool YIsUpAxis;
        public bool RecreateCellHierarchy;

        /// <summary>
        ///     This represents the first ID which is assigned to the first created cell.
        ///     If you already have some interest groups blocking this first ID, fell free to change it.
        ///     However increasing the first group ID decreases the maximum amount of allowed cells.
        ///     Allowed values are in range from 1 to 250.
        /// </summary>
        public readonly byte FIRST_GROUP_ID = 1;

        /// <summary>
        ///     This represents the order in which updates are sent.
        ///     The number represents the subdivision of the cell hierarchy:
        ///     - 0: message is sent to all players
        ///     - 1: message is sent to players who are interested in the matching cell of the first subdivision
        ///     If there is only one subdivision we are sending one update to all players
        ///     before sending three consequent updates only to players who are in the same cell
        ///     or interested in updates of the current cell.
        /// </summary>
        public readonly int[] SUBDIVISION_FIRST_LEVEL_ORDER = new int[4] { 0, 1, 1, 1 };

        /// <summary>
        ///     This represents the order in which updates are sent.
        ///     The number represents the subdivision of the cell hierarchy:
        ///     - 0: message is sent to all players
        ///     - 1: message is sent to players who are interested in the matching cell of the first subdivision
        ///     - 2: message is sent to players who are interested in the matching cell of the second subdivision
        ///     If there are two subdivisions we are sending every second update only to players
        ///     who are in the same cell or interested in updates of the current cell.
        /// </summary>
        public readonly int[] SUBDIVISION_SECOND_LEVEL_ORDER = new int[8] { 0, 2, 1, 2, 0, 2, 1, 2 };

        /// <summary>
        ///     This represents the order in which updates are sent.
        ///     The number represents the subdivision of the cell hierarchy:
        ///     - 0: message is sent to all players
        ///     - 1: message is sent to players who are interested in the matching cell of the first subdivision
        ///     - 2: message is sent to players who are interested in the matching cell of the second subdivision
        ///     - 3: message is sent to players who are interested in the matching cell of the third subdivision
        ///     If there are two subdivisions we are sending every second update only to players
        ///     who are in the same cell or interested in updates of the current cell.
        /// </summary>
        public readonly int[] SUBDIVISION_THIRD_LEVEL_ORDER = new int[12] { 0, 3, 2, 3, 1, 3, 2, 3, 1, 3, 2, 3 };

        private byte idCounter;

        public int CellCount { get; private set; }

        public CellTree CellTree { get; private set; }

        public Dictionary<int, GameObject> Map { get; }

        /// <summary>
        ///     Creates the cell hierarchy at runtime.
        /// </summary>
        private void Awake()
        {
            idCounter = FIRST_GROUP_ID;

            CreateCellHierarchy();
        }

        /// <summary>
        ///     Creates the cell hierarchy in editor and draws the cell view.
        /// </summary>
        public void OnDrawGizmos()
        {
            idCounter = FIRST_GROUP_ID;

            if (RecreateCellHierarchy) CreateCellHierarchy();

            DrawCells();
        }

        /// <summary>
        ///     Creates the cell hierarchy.
        /// </summary>
        private void CreateCellHierarchy()
        {
            if (!IsCellCountAllowed())
            {
                if (Debug.isDebugBuild)
                {
                    Debug.LogError(
                        "There are too many cells created by your subdivision options. Maximum allowed number of cells is " +
                        (MAX_NUMBER_OF_ALLOWED_CELLS - FIRST_GROUP_ID) +
                        ". Current number of cells is " + CellCount + ".");
                    return;
                }

                Application.Quit();
            }

            var rootNode = new CellTreeNode(idCounter++, CellTreeNode.ENodeType.Root, null);

            if (YIsUpAxis)
            {
                Center = new Vector2(transform.position.x, transform.position.y);
                Size = new Vector2(transform.localScale.x, transform.localScale.y);

                rootNode.Center = new Vector3(Center.x, Center.y, 0.0f);
                rootNode.Size = new Vector3(Size.x, Size.y, 0.0f);
                rootNode.TopLeft = new Vector3(Center.x - Size.x / 2.0f, Center.y - Size.y / 2.0f, 0.0f);
                rootNode.BottomRight = new Vector3(Center.x + Size.x / 2.0f, Center.y + Size.y / 2.0f, 0.0f);
            }
            else
            {
                Center = new Vector2(transform.position.x, transform.position.z);
                Size = new Vector2(transform.localScale.x, transform.localScale.z);

                rootNode.Center = new Vector3(Center.x, 0.0f, Center.y);
                rootNode.Size = new Vector3(Size.x, 0.0f, Size.y);
                rootNode.TopLeft = new Vector3(Center.x - Size.x / 2.0f, 0.0f, Center.y - Size.y / 2.0f);
                rootNode.BottomRight = new Vector3(Center.x + Size.x / 2.0f, 0.0f, Center.y + Size.y / 2.0f);
            }

            CreateChildCells(rootNode, 1);

            CellTree = new CellTree(rootNode);

            RecreateCellHierarchy = false;
        }

        /// <summary>
        ///     Creates all child cells.
        /// </summary>
        /// <param name="parent">The current parent node.</param>
        /// <param name="cellLevelInHierarchy">The cell level within the current hierarchy.</param>
        private void CreateChildCells(CellTreeNode parent, int cellLevelInHierarchy)
        {
            if (cellLevelInHierarchy > NumberOfSubdivisions) return;

            var rowCount = (int)Subdivisions[cellLevelInHierarchy - 1].x;
            var columnCount = (int)Subdivisions[cellLevelInHierarchy - 1].y;

            var startX = parent.Center.x - parent.Size.x / 2.0f;
            var width = parent.Size.x / rowCount;

            for (var row = 0; row < rowCount; ++row)
            for (var column = 0; column < columnCount; ++column)
            {
                var xPos = startX + row * width + width / 2.0f;

                var node = new CellTreeNode(idCounter++,
                    NumberOfSubdivisions == cellLevelInHierarchy
                        ? CellTreeNode.ENodeType.Leaf
                        : CellTreeNode.ENodeType.Node, parent);

                if (YIsUpAxis)
                {
                    var startY = parent.Center.y - parent.Size.y / 2.0f;
                    var height = parent.Size.y / columnCount;
                    var yPos = startY + column * height + height / 2.0f;

                    node.Center = new Vector3(xPos, yPos, 0.0f);
                    node.Size = new Vector3(width, height, 0.0f);
                    node.TopLeft = new Vector3(xPos - width / 2.0f, yPos - height / 2.0f, 0.0f);
                    node.BottomRight = new Vector3(xPos + width / 2.0f, yPos + height / 2.0f, 0.0f);
                }
                else
                {
                    var startZ = parent.Center.z - parent.Size.z / 2.0f;
                    var depth = parent.Size.z / columnCount;
                    var zPos = startZ + column * depth + depth / 2.0f;

                    node.Center = new Vector3(xPos, 0.0f, zPos);
                    node.Size = new Vector3(width, 0.0f, depth);
                    node.TopLeft = new Vector3(xPos - width / 2.0f, 0.0f, zPos - depth / 2.0f);
                    node.BottomRight = new Vector3(xPos + width / 2.0f, 0.0f, zPos + depth / 2.0f);
                }

                parent.AddChild(node);

                CreateChildCells(node, cellLevelInHierarchy + 1);
            }
        }

        /// <summary>
        ///     Draws the cells.
        /// </summary>
        private void DrawCells()
        {
            if (CellTree != null && CellTree.RootNode != null)
                CellTree.RootNode.Draw();
            else
                RecreateCellHierarchy = true;
        }

        /// <summary>
        ///     Checks if the cell count is allowed.
        /// </summary>
        /// <returns>True if the cell count is allowed, false if the cell count is too large.</returns>
        private bool IsCellCountAllowed()
        {
            var horizontalCells = 1;
            var verticalCells = 1;

            foreach (var v in Subdivisions)
            {
                horizontalCells *= (int)v.x;
                verticalCells *= (int)v.y;
            }

            CellCount = horizontalCells * verticalCells;

            return CellCount <= MAX_NUMBER_OF_ALLOWED_CELLS - FIRST_GROUP_ID;
        }

        /// <summary>
        ///     Gets a list of all cell IDs the player is currently inside or nearby.
        /// </summary>
        /// <param name="position">The current position of the player.</param>
        /// <returns>A list containing all cell IDs the player is currently inside or nearby.</returns>
        public List<byte> GetActiveCells(Vector3 position)
        {
            var activeCells = new List<byte>(0);
            CellTree.RootNode.GetActiveCells(activeCells, YIsUpAxis, position);

            // it makes sense to sort the "nearby" cells. those are in the list in positions after the subdivisions the point is inside. 2 subdivisions result in 3 areas the point is in.
            var cellsActive = NumberOfSubdivisions + 1;
            var cellsNearby = activeCells.Count - cellsActive;
            if (cellsNearby > 0) activeCells.Sort(cellsActive, cellsNearby, new ByteComparer());
            return activeCells;
        }
    }

    /// <summary>
    ///     Represents the tree accessible from its root node.
    /// </summary>
    public class CellTree
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public CellTree()
        {
        }

        /// <summary>
        ///     Constructor to define the root node.
        /// </summary>
        /// <param name="root">The root node of the tree.</param>
        public CellTree(CellTreeNode root)
        {
            RootNode = root;
        }

        /// <summary>
        ///     Represents the root node of the cell tree.
        /// </summary>
        public CellTreeNode RootNode { get; }
    }

    /// <summary>
    ///     Represents a single node of the tree.
    /// </summary>
    public class CellTreeNode
    {
        public enum ENodeType : byte
        {
            Root = 0,
            Node = 1,
            Leaf = 2
        }

        /// <summary>
        ///     Represents the center, top-left or bottom-right position of the cell
        ///     or the size of the cell.
        /// </summary>
        public Vector3 Center, Size, TopLeft, BottomRight;

        /// <summary>
        ///     A list containing all child nodes.
        /// </summary>
        public List<CellTreeNode> Childs;

        /// <summary>
        ///     Represents the unique ID of the cell.
        /// </summary>
        public byte Id;

        /// <summary>
        ///     The max distance the player can have to the center of the cell for being 'nearby'.
        ///     This is calculated once at runtime.
        /// </summary>
        private float maxDistance;

        /// <summary>
        ///     Describes the current node type of the cell tree node.
        /// </summary>
        public ENodeType NodeType;

        /// <summary>
        ///     Reference to the parent node.
        /// </summary>
        public CellTreeNode Parent;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public CellTreeNode()
        {
        }

        /// <summary>
        ///     Constructor to define the ID and the node type as well as setting a parent node.
        /// </summary>
        /// <param name="id">The ID of the cell is used as the interest group.</param>
        /// <param name="nodeType">The node type of the cell tree node.</param>
        /// <param name="parent">The parent node of the cell tree node.</param>
        public CellTreeNode(byte id, ENodeType nodeType, CellTreeNode parent)
        {
            Id = id;

            NodeType = nodeType;

            Parent = parent;
        }

        /// <summary>
        ///     Adds the given child to the node.
        /// </summary>
        /// <param name="child">The child which is added to the node.</param>
        public void AddChild(CellTreeNode child)
        {
            if (Childs == null) Childs = new List<CellTreeNode>(1);

            Childs.Add(child);
        }

        /// <summary>
        ///     Draws the cell in the editor.
        /// </summary>
        public void Draw()
        {
#if UNITY_EDITOR
            if (Childs != null)
                foreach (var node in Childs)
                    node.Draw();

            Gizmos.color = new Color(NodeType == ENodeType.Root ? 1 : 0, NodeType == ENodeType.Node ? 1 : 0,
                NodeType == ENodeType.Leaf ? 1 : 0);
            Gizmos.DrawWireCube(Center, Size);

            var offset = (byte)NodeType;
            var gs = new GUIStyle { fontStyle = FontStyle.Bold };
            gs.normal.textColor = Gizmos.color;
            Handles.Label(Center + Vector3.forward * offset * 1f, Id.ToString(), gs);
#endif
        }

        /// <summary>
        ///     Gathers all cell IDs the player is currently inside or nearby.
        /// </summary>
        /// <param name="activeCells">The list to add all cell IDs to the player is currently inside or nearby.</param>
        /// <param name="yIsUpAxis">Describes if the y-axis is used as up-axis.</param>
        /// <param name="position">The current position of the player.</param>
        public void GetActiveCells(List<byte> activeCells, bool yIsUpAxis, Vector3 position)
        {
            if (NodeType != ENodeType.Leaf)
            {
                foreach (var node in Childs) node.GetActiveCells(activeCells, yIsUpAxis, position);
            }
            else
            {
                if (IsPointNearCell(yIsUpAxis, position))
                {
                    if (IsPointInsideCell(yIsUpAxis, position))
                    {
                        activeCells.Insert(0, Id);

                        var p = Parent;
                        while (p != null)
                        {
                            activeCells.Insert(0, p.Id);

                            p = p.Parent;
                        }
                    }
                    else
                    {
                        activeCells.Add(Id);
                    }
                }
            }
        }

        /// <summary>
        ///     Checks if the given point is inside the cell.
        /// </summary>
        /// <param name="yIsUpAxis">Describes if the y-axis is used as up-axis.</param>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is inside the cell, false if the point is not inside the cell.</returns>
        public bool IsPointInsideCell(bool yIsUpAxis, Vector3 point)
        {
            if (point.x < TopLeft.x || point.x > BottomRight.x) return false;

            if (yIsUpAxis)
            {
                if (point.y >= TopLeft.y && point.y <= BottomRight.y) return true;
            }
            else
            {
                if (point.z >= TopLeft.z && point.z <= BottomRight.z) return true;
            }

            return false;
        }

        /// <summary>
        ///     Checks if the given point is near the cell.
        /// </summary>
        /// <param name="yIsUpAxis">Describes if the y-axis is used as up-axis.</param>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is near the cell, false if the point is too far away.</returns>
        public bool IsPointNearCell(bool yIsUpAxis, Vector3 point)
        {
            if (maxDistance == 0.0f) maxDistance = (Size.x + Size.y + Size.z) / 2.0f;

            return (point - Center).sqrMagnitude <= maxDistance * maxDistance;
        }
    }


    public class ByteComparer : IComparer<byte>
    {
        /// <inheritdoc />
        public int Compare(byte x, byte y)
        {
            return x == y ? 0 : x < y ? -1 : 1;
        }
    }
}
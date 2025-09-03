using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace GitForce
{
    /// <summary>
    /// Yet another TreeViewEx control, written to be suitable for this project.
    ///
    /// Adds to the standard TreeView the following features:
    /// * Multi-selection via new property SelectedNodes[]
    /// * Deep selection: recursive selection into children nodes
    ///
    /// Important: Instead of "nodes.Clear()", call this NodesClear() function!
    /// </summary>
    public class TreeViewEx : TreeView
    {
        /// <summary>
        /// Mask flickering on TreeView:
        /// http://stackoverflow.com/questions/10362988/treeview-flickering
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            if (!ClassUtils.IsMono()) // Can't send message (and use a DLL) on Mono
                SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }

        // Pinvoke:
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        /// <summary>
        /// Private list of selected nodes
        /// </summary>
        private readonly List<TreeNode> _selectedNodes;

		/// <summary>
		/// Public property of the TreeViewEx to return or set a list of selected nodes
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public List<TreeNode> SelectedNodes
        {
            get { return _selectedNodes; }
            set { SetSelectedNodes(value); }
        }

		/// <summary>
		/// Return a single selected node
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new TreeNode SelectedNode { get; set; }

        /// <summary>
        /// TreeViewEx constructor
        /// </summary>
        public TreeViewEx()
        {
            _selectedNodes = new List<TreeNode>();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            TreeNode node = GetNodeAt(e.Location);
            if (node!=null)
            {
                BeginUpdate();
                if (ModifierKeys == Keys.None)
                {
                    if (!_selectedNodes.Contains(node))
                    {
                        SelectNone();
                        SetSelected(node, true);
                        SelectedNode = node;
                    }
                }
                if (ModifierKeys == Keys.Control)
                {
                    SetSelectedDeep(node, !_selectedNodes.Contains(node));
                    SelectedNode = node;
                }
                if (ModifierKeys == Keys.Shift)
                {
                    // Select all nodes from the last selected one to the current one
                    // Under the same parent, recursively into their children nodes
                    if (node.Parent != null)
                    {
                        List<TreeNode> siblings = node.Parent.Nodes.Cast<TreeNode>().ToList();
                        if (siblings.Contains(SelectedNode))
                        {
                            int isel = siblings.IndexOf(SelectedNode);
                            int icur = siblings.IndexOf(node);
                            int istart = Math.Min(isel, icur);
                            int iend = istart + Math.Abs(isel - icur) + 1;
                            for (int i = istart; i < iend; i++)
                                SetSelected(siblings[i], true);
                        }
                    }
                }
                EndUpdate();
            }
            base.OnMouseDown(e);
            OnAfterSelect(null);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            TreeNode node = GetNodeAt(e.Location);
            if (node != null)
            {
                BeginUpdate();
                if (ModifierKeys == Keys.None && e.Button==MouseButtons.Left)
                {
                    SelectNone();
                    SetSelected(node, true);
                    SelectedNode = node;
                    OnAfterSelect(null);
                }
                EndUpdate();
            }
            base.OnMouseUp(e);
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            base.SelectedNode = null;
            e.Cancel = true;
            base.OnBeforeSelect(e);
        }

        protected override void OnItemDrag(ItemDragEventArgs e)
        {
            var node = e.Item as TreeNode;
            if (node != null)
            {
                if (!_selectedNodes.Contains(node))
                {
                    SelectNone();
                    SetSelected(node, true);
                    OnAfterSelect(null);
                }
            }
            base.OnItemDrag(e);
        }

        /// <summary>
        /// Select or deselect a given node
        /// </summary>
        private void SetSelected(TreeNode node, bool bSelect)
        {
            if (bSelect)
            {
                if (!_selectedNodes.Contains(node))
                    _selectedNodes.Add(node);

                node.BackColor = SystemColors.Highlight;
                node.ForeColor = SystemColors.HighlightText;
            }
            else
            {
                _selectedNodes.Remove(node);
                node.BackColor = BackColor;
                node.ForeColor = ForeColor;
            }
        }

        /// <summary>
        /// Deep selection sets the given node and all of its children, recursively.
        /// </summary>
        private void SetSelectedDeep(TreeNode node, bool bSelect)
        {
            foreach (TreeNode n in node.Nodes)
                SetSelectedDeep(n, bSelect);
            SetSelected(node, bSelect);
        }

        /// <summary>
        /// Select every node in the tree
        /// </summary>
        public void SelectAll()
        {
            BeginUpdate();
            foreach (TreeNode node in Nodes)
                SetSelectedDeep(node, true);
            EndUpdate();
            OnAfterSelect(null);
        }

        /// <summary>
        /// Deselect all nodes in the tree
        /// </summary>
        private void SelectNone()
        {
            BeginUpdate();
            foreach (TreeNode node in Nodes)
                SetSelectedDeep(node, false);
            EndUpdate();
            OnAfterSelect(null);
        }

        /// <summary>
        /// Clear the tree from all the nodes. Use this call instead of treeView.Nodes.Clear()
        /// </summary>
        public void NodesClear()
        {
            _selectedNodes.Clear();
            Nodes.Clear();
            OnAfterSelect(null);
        }

        /// <summary>
        /// Alternate way to select specific nodes by sending it a list
        /// </summary>
        private void SetSelectedNodes(List<TreeNode> nodes)
        {
            SelectNone();
            BeginUpdate();
            foreach (TreeNode n in nodes)
            {
                SetSelected(n, true);
                n.EnsureVisible();
            }
            EndUpdate();
            OnAfterSelect(null);
        }
    }
}

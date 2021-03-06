extern alias uiaComWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using uiaComWrapper::System.Windows.Automation;
using WindowsAccessBridgeInterop;

namespace Pixel.Automation.Java.Access.Bridge.Components
{
    public static class AccessibleNodeExtension
    {
        /// <summary>
        /// Find the first element inside the root node that matches specified control details.
        /// Lookup is performed only within provided TreeScope
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="searchScope"></param>
        /// <param name="controlDetails"></param>
        /// <returns></returns>
        public static AccessibleContextNode FindFirst(this AccessibleContextNode rootNode, TreeScope searchScope, JavaControlIdentity controlDetails)
        {
            AccessibleContextNode foundNode = default(AccessibleContextNode);
            switch (searchScope)
            {
                case TreeScope.Element:
                    if (rootNode.IsMatch(controlDetails))
                    {
                        foundNode = rootNode;
                    }
                    break;
                case TreeScope.Children:
                    foundNode = FindChildNode(rootNode, controlDetails);
                    break;
                case TreeScope.Element | TreeScope.Children:
                    if (rootNode.IsMatch(controlDetails))
                    {
                        foundNode = rootNode;
                    }
                    else
                    {
                        foundNode = FindChildNode(rootNode, controlDetails);
                    }
                    break;
                case TreeScope.Descendants:            
                case TreeScope.Children | TreeScope.Descendants:
                    foundNode = FindDescendantNode(rootNode, controlDetails);
                    break;
                case TreeScope.Subtree:
                    if (rootNode.IsMatch(controlDetails))
                    {
                        foundNode = rootNode;
                    }
                    else
                    {
                        foundNode = FindDescendantNode(rootNode, controlDetails);
                    }
                    break;
                case TreeScope.Ancestors:
                    var ancestor = rootNode.GetParent();
                    while (ancestor != null && ancestor is AccessibleContextNode ancestorNode)
                    {
                        if (ancestorNode.IsMatch(controlDetails))
                        {
                            foundNode = ancestorNode;
                        }
                        ancestor = ancestor.GetParent();
                    }
                    break;
                default:
                    throw new NotSupportedException($"TreeScope {searchScope} is not supported");
            }
            return foundNode;
        }

        /// <summary>
        /// Find the nth matching descendant control.
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="javaControlIdentity"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static AccessibleContextNode FindNthDescendantControl(this AccessibleContextNode rootNode, JavaControlIdentity javaControlIdentity, int index)
        {
            var currentSearchRoot = rootNode;
            Queue<AccessibleContextNode> queue = new Queue<AccessibleContextNode>();
            List<AccessibleContextNode> matchingDescendants = new List<AccessibleContextNode>();
            queue.Enqueue(currentSearchRoot);
            while (queue.Count > 0)
            {
                AccessibleContextNode node = queue.Dequeue();
                foreach (AccessibleContextNode childNode in node.GetChildren())
                {
                    if (IsMatch(childNode, javaControlIdentity))
                    {
                        matchingDescendants.Add(childNode);
                        //Return as soon as we have found nth match. No need to look further
                        if (matchingDescendants.Count() == index)
                        {
                            return childNode;
                        }                        
                    }
                    queue.Enqueue(childNode);
                }
            }
            return null;
        }

        /// <summary>
        /// Find the index of the control relative to other controls having same identifiers within the ancestor node.
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="ancestorNode"></param>
        /// <returns></returns>
        public static int? FindIndexOfControl(this AccessibleContextNode currentNode, AccessibleContextNode ancestorNode)
        {
            var currentNodeInfo = currentNode.GetInfo();
            var controlIdentity = new JavaControlIdentity() { ControlName = currentNodeInfo.name, Description = currentNodeInfo.description, Role = currentNodeInfo.role };
            int index = 1;
            foreach(var matchingNode in ancestorNode.FindAll(TreeScope.Descendants, controlIdentity))
            {
                if(matchingNode.Equals(currentNode))
                {
                    break;
                }
                index++;
            }
            if(index > 1)
            {
                return index;
            }
            return null;
        }

        /// <summary>
        /// Find all the elements inside the root node that matches specified control details
        /// Lookup is performed only within provided TreeScope
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="searchScope"></param>
        /// <param name="controlDetails"></param>
        /// <returns></returns>
        public static IEnumerable<AccessibleContextNode> FindAll(this AccessibleContextNode currentNode, TreeScope searchScope, JavaControlIdentity controlDetails)
        {
            List<AccessibleContextNode> foundNodes = new List<AccessibleContextNode>();
            switch (searchScope)
            {
                case TreeScope.Element:
                    if (currentNode.IsMatch(controlDetails))
                    {
                        foundNodes.Add(currentNode);
                    }
                    break;

                case TreeScope.Children:
                    foreach (AccessibleContextNode node in FindAllChildNode(currentNode, controlDetails))
                    {
                        foundNodes.Add(node);
                    }
                    break;
                case TreeScope.Element | TreeScope.Children:
                    if (currentNode.IsMatch(controlDetails))
                    {
                        foundNodes.Add(currentNode);
                    }
                    foreach (AccessibleContextNode node in FindAllChildNode(currentNode, controlDetails))
                    {
                        foundNodes.Add(node);
                    }
                    break;
                   
                case TreeScope.Descendants:
                case TreeScope.Children | TreeScope.Descendants:
                    foreach(AccessibleContextNode node in FindAllDescendantNodes(currentNode, controlDetails))
                    {
                        foundNodes.Add(node);
                    }
                    break;

                case TreeScope.Subtree:
                    if (currentNode.IsMatch(controlDetails))
                        foundNodes.Add(currentNode);
                    foreach (AccessibleContextNode node in FindAllDescendantNodes(currentNode, controlDetails))
                    {
                        foundNodes.Add(node);
                    }
                    break;
                default:
                    throw new NotSupportedException($"TreeScope {searchScope} is not supported");
            }

            return foundNodes;
        }


        private static AccessibleContextNode FindChildNode(AccessibleContextNode rootNode, JavaControlIdentity controlDetails)
        {
            AccessibleContextNode foundNode = default;
            foreach (AccessibleContextNode node in rootNode.GetChildren())
            {
                if (node.IsMatch(controlDetails))
                {
                    foundNode = node;
                    break;
                }
              
            }
            return foundNode;
        }

        private static IEnumerable<AccessibleContextNode> FindAllChildNode(AccessibleContextNode rootNode, JavaControlIdentity controlDetails)
        {
            foreach (AccessibleContextNode childNode in rootNode.GetChildren())
            {
                if (childNode.IsMatch(controlDetails))
                {
                   yield return childNode;
                }
            }
            yield break;
        }

        /// <summary>
        /// Find the first descendant node that match the specified control details in a given root node 
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="controlDetails"></param>
        /// <returns></returns>
        private static AccessibleContextNode FindDescendantNode(AccessibleContextNode rootNode, JavaControlIdentity controlDetails)
        {
            Queue<AccessibleContextNode> queue = new Queue<AccessibleContextNode>();
            queue.Enqueue(rootNode);
            while(queue.Count > 0)
            {
                AccessibleContextNode node = queue.Dequeue();
                foreach (AccessibleContextNode childNode in node.GetChildren())
                {
                    if (childNode.IsMatch(controlDetails))
                    {
                        return childNode;
                    }
                    queue.Enqueue(childNode);
                }
            }
            return null;
        }       

        /// <summary>
        /// Find All Descendant nodes that match the specified control details in a given root node using breadth first lookup 
        /// </summary>
        /// <param name="rootNode"></param>
        /// <param name="controlDetails"></param>
        /// <returns></returns>
        private static IEnumerable<AccessibleContextNode> FindAllDescendantNodes(AccessibleContextNode rootNode, JavaControlIdentity controlDetails)
        {
            Queue<AccessibleContextNode> queue = new Queue<AccessibleContextNode>();
            queue.Enqueue(rootNode);
            while (queue.Count > 0)
            {
                AccessibleContextNode node = queue.Dequeue();
                foreach (AccessibleContextNode childNode in node.GetChildren())
                {
                    if (childNode.IsMatch(controlDetails))
                    {
                        yield return childNode;
                    }
                    queue.Enqueue(childNode);
                }
            }         
            yield break;
        }

        /// <summary>
        /// Checks whether the AccessibleContextNode matches the description of specified control details
        /// </summary>
        /// <param name="node"></param>
        /// <param name="controlIdentity"></param>
        /// <returns></returns>
        public static bool IsMatch(this AccessibleContextNode node, JavaControlIdentity controlIdentity)
        {
            AccessibleContextInfo nodeInfo = node.GetInfo();

            bool isMatch = true;
            if (!string.IsNullOrEmpty(controlIdentity.ControlName))
            {
                isMatch = isMatch && nodeInfo.name.Equals(controlIdentity.ControlName);
            }
            if (!string.IsNullOrEmpty(controlIdentity.Description))
            {
                isMatch = isMatch && nodeInfo.description.Equals(controlIdentity.Description);
            }
            //Expected that control always have a role atleast
            isMatch = isMatch && nodeInfo.role.Equals(controlIdentity.Role);
            return isMatch;
        }

        /// <summary>
        /// Get the number of descendant elements of a given AccessibleNode
        /// </summary>
        /// <param name="accessibleNode"></param>
        /// <returns></returns>
        public static int GetDescendantsCount(this AccessibleNode accessibleNode)
        {
            int i = 0;
            foreach (var accessibleChildNode  in accessibleNode.GetChildren())
            {
                i++;
                i += accessibleChildNode.GetDescendantsCount();               
            }
            return i;
        }
    }
}

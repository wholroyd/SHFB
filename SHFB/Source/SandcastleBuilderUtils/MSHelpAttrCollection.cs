//===============================================================================================================
// System  : Sandcastle Help File Builder Utilities
// File    : MSHelpAttrCollection.cs
// Author  : Eric Woodruff  (Eric@EWoodruff.us)
// Updated : 05/15/2015
// Note    : Copyright 2008-2015, Eric Woodruff, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a collection class used to hold the help attribute information
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code.  It can also be found at the project website: https://GitHub.com/EWSoftware/SHFB.  This
// notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who  Comments
// ==============================================================================================================
// 03/25/2008  EFW  Created the code
// 07/03/2008  EFW  Rewrote to support MSBuild project format
// 04/07/2011  EFW  Made the constructor and from/to XML members public so that it can be used from the VSPackage
//===============================================================================================================

using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;

namespace SandcastleBuilder.Utils
{
    /// <summary>
    /// This collection class is used to hold the help attribute items for a project
    /// </summary>
    public class MSHelpAttrCollection : BindingList<MSHelpAttr>
    {
        #region Sort collection
        //=====================================================================

        /// <summary>
        /// This is used to sort the collection
        /// </summary>
        /// <remarks>Values are sorted by attribute name and value</remarks>
        public void Sort()
        {
            ((List<MSHelpAttr>)base.Items).Sort((x, y) => Comparer<MSHelpAttr>.Default.Compare(x, y));
        }
        #endregion

        #region Read/write help attribute items from/to XML
        //=====================================================================

        /// <summary>
        /// This is used to load existing help attribute items from the project file
        /// </summary>
        /// <param name="helpAttrs">The help attribute items</param>
        /// <remarks>The information is stored as an XML fragment</remarks>
        public void FromXml(string helpAttrs)
        {
            using(var xr = new XmlTextReader(helpAttrs, XmlNodeType.Element,
              new XmlParserContext(null, null, null, XmlSpace.Default)))
            {
                xr.MoveToContent();

                this.ReadXml(xr);
            }
        }

        /// <summary>
        /// Load the help attributes from the given XML text reader
        /// </summary>
        /// <param name="xr">The XML reader to use</param>
        internal void ReadXml(XmlReader xr)
        {
            while(!xr.EOF && xr.NodeType != XmlNodeType.EndElement)
            {
                if(xr.NodeType == XmlNodeType.Element && xr.Name == "HelpAttribute")
                    this.Add(xr.GetAttribute("name"), xr.GetAttribute("value"));

                xr.Read();
            }
        }

        /// <summary>
        /// This is used to write the help attribute info to an XML fragment ready for storing in the project
        /// file.
        /// </summary>
        /// <returns>The XML fragment containing the help attribute info</returns>
        public string ToXml()
        {
            using(var ms = new MemoryStream(10240))
            {
                using(var xw = new XmlTextWriter(ms, new UTF8Encoding(false)))
                {
                    xw.Formatting = Formatting.Indented;
                    this.WriteXml(xw, false);
                    xw.Flush();

                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// Write the collection to the given XML text writer
        /// </summary>
        /// <param name="xw">The XML text writer to which the information is written.</param>
        /// <param name="includeContainer">True to write out the containing <b>HelpAttributes</b> element, false
        /// to exclude it.</param>
        internal void WriteXml(XmlWriter xw, bool includeContainer)
        {
            if(includeContainer)
                xw.WriteStartElement("HelpAttributes");

            foreach(MSHelpAttr attr in this)
            {
                xw.WriteStartElement("HelpAttribute");
                xw.WriteAttributeString("name", attr.AttributeName);
                xw.WriteAttributeString("value", attr.AttributeValue);
                xw.WriteEndElement();
            }

            if(includeContainer)
                xw.WriteEndElement();
        }
        #endregion

        #region Add/create a new help attribute item
        //=====================================================================

        /// <summary>
        /// Add a new item to the collection
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value</param>
        /// <returns>The <see cref="MSHelpAttr" /> added to the project.  If the item already exists in the
        /// collection, the existing item is returned.</returns>
        /// <remarks>The <see cref="MSHelpAttr" /> constructor is internal so that we control creation of the
        /// items and can associate them with the project.</remarks>
        public MSHelpAttr Add(string name, string value)
        {
            MSHelpAttr item = new MSHelpAttr(name, value);

            if(!this.Contains(item))
                base.Add(item);

            return item;
        }
        #endregion

        #region Convert the collection to a string for the configuration file
        //=====================================================================

        /// <summary>
        /// Convert the collection to its string form for use in the <b>sandcastle.config</b> file
        /// </summary>
        /// <returns>The help attribute collection in string form ready for use in the Sandcastle BuildAssembler
        /// configuration file.</returns>
        public string ToConfigurationString()
        {
            StringBuilder sb = new StringBuilder("<attributes>\r\n", 1024);

            foreach(MSHelpAttr ha in this)
                sb.AppendFormat("  <attribute name=\"{0}\" value=\"{1}\" />", ha.AttributeName,
                    HttpUtility.HtmlEncode(ha.AttributeValue));

            sb.Append("</attributes>\r\n");

            return sb.ToString();
        }
        #endregion
    }
}

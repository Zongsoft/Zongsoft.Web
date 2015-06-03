/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2015 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Web.
 *
 * Zongsoft.Web is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Web is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Web; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

namespace Zongsoft.Web.Controls
{
	[DefaultProperty("Nodes")]
	[PersistChildren(true)]
	[ParseChildren(true)]
	public class TreeView : CompositeDataBoundControl
	{
		#region 成员变量
		private ITemplate _emptyTemplate;
		private ITemplate _nodeTemplate;
		private TreeViewNodeCollection _nodes;
		#endregion

		#region 构造函数
		public TreeView()
		{
			this.CssClass = "ui list";
		}
		#endregion

		#region 公共属性
		[DefaultValue(ListRenderMode.List)]
		[PropertyMetadata(false)]
		public ListRenderMode RenderMode
		{
			get
			{
				return this.GetPropertyValue(() => this.RenderMode);
			}
			set
			{
				this.SetPropertyValue(() => this.RenderMode, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string LoadingPath
		{
			get
			{
				return this.GetPropertyValue(() => this.LoadingPath);
			}
			set
			{
				this.SetPropertyValue(() => this.LoadingPath, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(ScrollbarMode.None)]
		[PropertyMetadata(false)]
		public ScrollbarMode ScrollbarMode
		{
			get
			{
				return this.GetPropertyValue(() => this.ScrollbarMode);
			}
			set
			{
				this.SetPropertyValue(() => this.ScrollbarMode, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(SelectionMode.None)]
		[PropertyMetadata(false)]
		public SelectionMode SelectionMode
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectionMode);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectionMode, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		[PropertyMetadata(false)]
		public string SelectedPath
		{
			get
			{
				return this.GetPropertyValue(() => this.SelectedPath);
			}
			set
			{
				this.SetPropertyValue(() => this.SelectedPath, value);
			}
		}

		[PropertyMetadata(false)]
		public bool HasNodes
		{
			get
			{
				return _nodes != null && _nodes.Count > 0;
			}
		}

		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TreeViewNodeCollection Nodes
		{
			get
			{
				if(_nodes == null)
					System.Threading.Interlocked.CompareExchange(ref _nodes, new TreeViewNodeCollection(), null);

				return _nodes;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(TreeView))]
		public ITemplate EmptyTemplate
		{
			get
			{
				return _emptyTemplate;
			}
			set
			{
				_emptyTemplate = value;
			}
		}

		[BrowsableAttribute(false)]
		[PersistenceModeAttribute(PersistenceMode.InnerProperty)]
		[TemplateContainerAttribute(typeof(TreeView))]
		public ITemplate NodeTemplate
		{
			get
			{
				return _nodeTemplate;
			}
			set
			{
				_nodeTemplate = value;
			}
		}

		[Bindable(true)]
		[TypeConverter(typeof(UnitConverter))]
		[PropertyMetadata(false)]
		public Unit Height
		{
			get
			{
				return this.GetPropertyValue(() => this.Height);
			}
			set
			{
				this.SetPropertyValue(() => this.Height, value);
			}
		}

		[Bindable(true)]
		[TypeConverter(typeof(UnitConverter))]
		[PropertyMetadata(false)]
		public Unit Width
		{
			get
			{
				return this.GetPropertyValue(() => this.Width);
			}
			set
			{
				this.SetPropertyValue(() => this.Width, value);
			}
		}
		#endregion

		#region 重写方法
		protected override void Render(HtmlTextWriter writer)
		{
			if((_nodes == null || _nodes.Count < 1) && this.DataSource == null)
			{
				if(_emptyTemplate != null)
					_emptyTemplate.InstantiateIn(this);
			}

			base.Render(writer);
		}

		protected override void RenderBeginTag(HtmlTextWriter writer)
		{
			var tagName = "div";

			switch(this.RenderMode)
			{
				case ListRenderMode.List:
					tagName = "dl";
					break;
				case ListRenderMode.BulletList:
				case ListRenderMode.OrderedList:
					tagName = "ul";
					break;
			}

			this.AddAttributes(writer);

			if(!Unit.IsEmpty(this.Height))
				writer.AddStyleAttribute(HtmlTextWriterStyle.Height, this.Height.ToString());

			if(!Unit.IsEmpty(this.Width))
				writer.AddStyleAttribute(HtmlTextWriterStyle.Width, this.Height.ToString());

			switch(this.ScrollbarMode)
			{
				case Web.Controls.ScrollbarMode.Horizontal:
					writer.AddStyleAttribute(HtmlTextWriterStyle.OverflowX, "scroll");
					break;
				case Web.Controls.ScrollbarMode.Vertical:
					writer.AddStyleAttribute(HtmlTextWriterStyle.OverflowY, "scroll");
					break;
				case Web.Controls.ScrollbarMode.Both:
					writer.AddStyleAttribute(HtmlTextWriterStyle.Overflow, "scroll");
					break;
			}

			writer.RenderBeginTag(tagName);
		}

		protected override void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		protected override void RenderContent(HtmlTextWriter writer)
		{
			if(_nodes == null || _nodes.Count < 1)
				return;

			for(int i = 0; i < _nodes.Count; i++)
			{
				this.RenderNode(writer, _nodes[i], i, 0);
			}

			if(string.IsNullOrWhiteSpace(this.LoadingPath))
				this.RenderDataNodes(writer, this.DataSource, 0, 0);
		}
		#endregion

		#region 公共方法
		public TreeViewNode Find(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				return null;

			var parts = path.Split('/');

			if(parts == null || parts.Length < 1)
				return null;

			TreeViewNode node = null;

			foreach(var part in parts)
			{
				if(string.IsNullOrWhiteSpace(part))
					continue;

				if(node == null)
					node = _nodes[part.Trim()];
				else
					node = node.Nodes[part.Trim()];

				if(node == null)
					return null;
			}

			return node;
		}
		#endregion

		#region 私有方法
		private void RenderNode(HtmlTextWriter writer, TreeViewNode node, int index, int depth)
		{
			if(node == null || (!node.Visible))
				return;

			string cssClass = "item";

			if(node.Selected)
				cssClass = Utility.ResolveCssClass(":selected", () => cssClass);

			if(node.Nodes.Count > 0)
				cssClass += " ui dropdown";

			if(!string.IsNullOrWhiteSpace(cssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);

			writer.RenderBeginTag(this.GetNodeTagName());

			if(node.Image != null)
				node.Image.ToHtmlString(writer);

			writer.AddAttribute(HtmlTextWriterAttribute.Href, string.IsNullOrWhiteSpace(node.Url) ? Utility.EmptyLink : node.Url);
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(node.Text);
			writer.RenderEndTag();

			if(node.Nodes.Count > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "menu");
				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				for(int i = 0; i < node.Nodes.Count; i++)
				{
					this.RenderNode(writer,
									node.Nodes[i],
									i, depth + 1);
				}

				writer.RenderEndTag();
			}

			if(!string.IsNullOrWhiteSpace(this.LoadingPath) && string.Equals(this.SelectedPath, node.FullPath, StringComparison.OrdinalIgnoreCase))
				this.RenderDataNodes(writer, this.DataSource, index, depth);

			writer.RenderEndTag();
		}

		private void RenderDataNodes(HtmlTextWriter writer, object dataSource, int index, int depth)
		{
			if(dataSource == null)
				return;

			this.RenderNodeTemplate(writer, new TreeViewNodeContainer(this, dataSource, index, this.GetNodeTagName(), "item")
			{
				Depth = depth,
			});

			var dataChildren = dataSource.GetType().GetProperty("").GetValue(dataSource);

			if(dataChildren == null)
				return;

			var dataItems = dataChildren as IEnumerable;

			if(dataChildren.GetType() == typeof(string) || dataItems == null)
			{
				this.RenderDataNodes(writer, dataChildren, 0, depth + 1);
			}
			else
			{
				int i = 0;

				foreach(var dataItem in dataItems)
				{
					this.RenderDataNodes(writer, dataItem, i++, depth + 1);
				}
			}
		}

		private void RenderNodeTemplate(HtmlTextWriter writer, TreeViewNodeContainer container)
		{
			if(_nodeTemplate != null)
			{
				_nodeTemplate.InstantiateIn(container);

				if(Utility.GetVisibleChildrenCount(container) > 0)
					container.RenderControl(writer);
			}
		}

		private string GetNodeTagName()
		{
			switch(this.RenderMode)
			{
				case ListRenderMode.None:
					return "div";
				case ListRenderMode.List:
					return "dt";
				case ListRenderMode.BulletList:
				case ListRenderMode.OrderedList:
					return "li";
			}

			return "div";
		}
		#endregion

		#region 嵌套子类
		internal class TreeViewNodeContainer : DataItemContainer<TreeView>
		{
			#region 成员字段
			private int _depth;
			#endregion

			#region 构造函数
			internal TreeViewNodeContainer(TreeView owner, object dataItem, int index, string tagName = null, string cssClass = null)
				: base(owner, dataItem, index, index, tagName, cssClass)
			{
			}

			internal TreeViewNodeContainer(TreeView owner, object dataItem, int index, int displayIndex, string tagName = null, string cssClass = null)
				: base(owner, dataItem, index, displayIndex, tagName, cssClass)
			{
			}
			#endregion

			#region 公共属性
			public int Depth
			{
				get
				{
					return _depth;
				}
				internal set
				{
					_depth = value;
				}
			}
			#endregion
		}
		#endregion
	}
}

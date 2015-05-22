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
		#region 私有常量
		private const int FIRSTNODE = 1;
		private const int LASTNODE = 2;
		#endregion

		#region 事件声明
		public event EventHandler<TreeViewNodeRenderEventArgs> NodeRender;
		#endregion

		#region 成员变量
		private TreeViewBinding _nodeBinding;
		private TreeViewBinding _rootNodeBinding;
		private TreeViewBinding _leafNodeBinding;
		private TreeViewNodeCollection _nodes;
		#endregion

		#region 构造函数
		public TreeView()
		{
			_nodes = new TreeViewNodeCollection();
		}
		#endregion

		#region 公共属性
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TreeViewBinding NodeBinding
		{
			get
			{
				if(_nodeBinding == null)
					System.Threading.Interlocked.CompareExchange(ref _nodeBinding, new TreeViewBinding(), null);

				return _nodeBinding;
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TreeViewBinding RootNodeBinding
		{
			get
			{
				if(_rootNodeBinding == null)
					System.Threading.Interlocked.CompareExchange(ref _rootNodeBinding, new TreeViewBinding(), null);

				return _rootNodeBinding;
			}
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TreeViewBinding LeafNodeBinding
		{
			get
			{
				if(_leafNodeBinding == null)
					System.Threading.Interlocked.CompareExchange(ref _leafNodeBinding, new TreeViewBinding(), null);

				return _leafNodeBinding;
			}
		}

		public TreeViewNodeCollection Nodes
		{
			get
			{
				return _nodes;
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
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

		[Bindable(true)]
		[TypeConverter(typeof(UnitConverter))]
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
			//生成数据源对应的子树
			this.GenerateNodes();

			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			if(string.IsNullOrWhiteSpace(this.CssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "tree");
			else
				writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);

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

			if(_nodes.Count > 0)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				for(int i = 0; i < _nodes.Count; i++)
				{
					this.RenderNode(writer,
									_nodes[i],
									i == 0 ? FIRSTNODE : (i == _nodes.Count - 1 ? LASTNODE : 0));
				}

				writer.RenderEndTag();
			}

			//调用基类同名方法
			base.Render(writer);
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

		#region 虚拟方法
		protected virtual void OnNodeRender(TreeViewNodeRenderEventArgs args)
		{
			if(this.NodeRender != null)
				this.NodeRender(this, args);
		}
		#endregion

		#region 私有方法
		private void GenerateNodes()
		{
			var loadingNode = this.Find(this.LoadingPath);

			if(loadingNode == null)
				this.GenerateNodes(this.DataSource, _nodes);
			else
				this.GenerateNodes(this.DataSource, loadingNode.Nodes);
		}

		private void GenerateNodes(object target, TreeViewNodeCollection nodes)
		{
			if(target == null || nodes == null)
				return;

			TreeViewBinding binding = nodes.Owner == null ? (_rootNodeBinding ?? _nodeBinding) : _nodeBinding;

			PropertyDescriptor childrenProperty = TypeDescriptor.GetProperties(target).Find(_nodeBinding.ChildrenPropertyName, true);
			if(childrenProperty == null)
				throw new InvalidOperationException(string.Format("The '{0}' children property is not found.", _nodeBinding.ChildrenPropertyName));

			IEnumerable children = childrenProperty.GetValue(target) as IEnumerable;
			if(children == null)
				throw new InvalidOperationException(string.Format("The '{0}' children property is not enumerable.", childrenProperty.Name));

			if(nodes.Owner == null)
				binding = _rootNodeBinding ?? _nodeBinding;
			else
			{
				if(Utility.IsEmpty(children))
					binding = _leafNodeBinding ?? _nodeBinding;
			}

			TreeViewNode node = this.GetNode(target, binding);
			if(node == null)
				return;

			nodes.Add(node);

			if((!string.IsNullOrWhiteSpace(this.SelectedPath)) && string.Equals(node.FullPath, this.SelectedPath, StringComparison.OrdinalIgnoreCase))
				node.Selected = true;

			foreach(object child in children)
			{
				if(child == null)
					continue;

				GenerateNodes(child, node.Nodes);
			}
		}

		private TreeViewNode GetNode(object target, TreeViewBinding binding)
		{
			if(target == null || binding == null)
				return null;

			string key = string.Empty;
			PropertyDescriptor property = TypeDescriptor.GetProperties(target).Find(binding.KeyPropertyName, true);

			if(property != null)
			{
				var value = property.GetValue(target);

				if(value != null)
					key = value.ToString();
			}

			TreeViewNode node = new TreeViewNode(key, key);

			node.Text = BindingUtility.FormatBindingValue(binding.Text, target);
			node.ToolTip = BindingUtility.FormatBindingValue(binding.ToolTip, target);
			node.Url = BindingUtility.FormatBindingValue(binding.Url, target);

			return node;
		}

		private void RenderNode(HtmlTextWriter writer, TreeViewNode node, int flag)
		{
			if(node == null || (!node.Visible))
				return;

			string cssClass = string.Empty;

			if(flag == FIRSTNODE)
				cssClass = Utility.GetCssClass(cssClass, "first");
			else if(flag == LASTNODE)
				cssClass = Utility.GetCssClass(cssClass, "last");

			if(node.Selected)
				cssClass = Utility.GetCssClass(cssClass, "selected");

			if(!string.IsNullOrWhiteSpace(cssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);

			writer.RenderBeginTag(HtmlTextWriterTag.Li);

			//writer.AddAttribute(HtmlTextWriterAttribute.Class, "tree-node");
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			if(!string.IsNullOrWhiteSpace(node.Icon))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, node.Icon.Trim() + " icon");
				writer.RenderBeginTag(HtmlTextWriterTag.I);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Href, string.IsNullOrWhiteSpace(node.Url) ? Utility.EmptyLink : node.Url);
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(node.Text);
			writer.RenderEndTag();

			//关闭<div class="tree-node">元素
			writer.RenderEndTag();

			if(node.Nodes.Count > 0)
			{
				writer.RenderBeginTag(HtmlTextWriterTag.Ul);

				for(int i = 0; i < node.Nodes.Count; i++)
				{
					this.RenderNode(writer,
									node.Nodes[i],
									i == 0 ? FIRSTNODE : (i == node.Nodes.Count - 1 ? LASTNODE : 0));
				}

				writer.RenderEndTag();
			}

			writer.RenderEndTag();
		}
		#endregion

		#region 嵌套子类
		[Serializable]
		public class TreeViewBinding
		{
			#region 成员变量
			private string _childrenPropertyName;
			#endregion

			#region 构造函数
			public TreeViewBinding()
			{
				_childrenPropertyName = "Children";
			}
			#endregion

			#region 公共属性
			public string ChildrenPropertyName
			{
				get
				{
					return _childrenPropertyName;
				}
				set
				{
					if(string.IsNullOrWhiteSpace(value))
						throw new ArgumentNullException();

					_childrenPropertyName = value.Trim();
				}
			}

			public string KeyPropertyName
			{
				get;
				set;
			}

			public string Text
			{
				get;
				set;
			}

			public string ToolTip
			{
				get;
				set;
			}

			public string Url
			{
				get;
				set;
			}
			#endregion
		}
		#endregion
	}
}

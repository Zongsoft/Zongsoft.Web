/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2011-2013 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Zongsoft.Web.Controls
{
	[Serializable]
	public class TreeViewNode
	{
		#region 成员变量
		private string _name;
		private string _text;
		private string _navigateUrl;
		private string _navigateCssClass;
		private string _toolTip;
		private string _description;
		private string _fullPath;
		private string _cssClass;
		private string _ListCssClass;
		private int _depth;
		private bool _selected;
		private bool _visible;
		private Image _image;
		private TreeView _treeView;
		private TreeViewNode _parent;
		private TreeViewNodeCollection _nodes;
		#endregion

		#region 构造函数
		public TreeViewNode()
		{
			_visible = true;
		}

		public TreeViewNode(string name, string text) : this(name, text, string.Empty)
		{
		}

		public TreeViewNode(string name, string text, string url)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if(name.Contains("/"))
			{
				if(name.Length == 1)
					name = "@";
				else
					throw new ArgumentException();
			}

			_name = name.Trim();
			_text = text ?? string.Empty;
			_navigateUrl = url ?? string.Empty;
			_toolTip = string.Empty;
			_description = string.Empty;
			_fullPath = string.Empty;
			_selected = false;
			_visible = true;
			_parent = null;
			_cssClass = "item";
		}
		#endregion

		#region 公共属性
		public TreeView TreeView
		{
			get
			{
				return _treeView;
			}
			internal set
			{
				_treeView = value;
			}
		}

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_name = value.Trim();
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				_text = value ?? string.Empty;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Please use the 'NavigateUrl' property.")]
		public string Url
		{
			get
			{
				return this.NavigateUrl;
			}
			set
			{
				this.NavigateUrl = value;
			}
		}

		public string NavigateUrl
		{
			get
			{
				return _navigateUrl;
			}
			set
			{
				if(value != null)
					_navigateUrl = value.Trim();
				else
					_navigateUrl = string.Empty;
			}
		}

		public string NavigateCssClass
		{
			get
			{
				return _navigateCssClass;
			}
			set
			{
				_navigateCssClass = value;
			}
		}

		public string Icon
		{
			get
			{
				return _image == null ? null : _image.Icon;
			}
			set
			{
				this.Image.Icon = value;
			}
		}

		public Image Image
		{
			get
			{
				if(_image == null)
					System.Threading.Interlocked.CompareExchange(ref _image, new Image(_treeView), null);

				return _image;
			}
			set
			{
				_image = value;
			}
		}

		public string ToolTip
		{
			get
			{
				return _toolTip;
			}
			set
			{
				_toolTip = value ?? string.Empty;
			}
		}

		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value ?? string.Empty;
			}
		}

		public string CssClass
		{
			get
			{
				return _cssClass;
			}
			set
			{
				_cssClass = value;
			}
		}

		public string ListCssClass
		{
			get
			{
				return _ListCssClass;
			}
			set
			{
				_ListCssClass = value;
			}
		}

		public int Depth
		{
			get
			{
				if(string.IsNullOrEmpty(_fullPath))
				{
					int depth = -1;
					TreeViewNode node = this;

					while(node != null)
					{
						depth++;
						node = node.Parent;
					}

					_depth = depth;
				}

				return _depth;
			}
		}

		public string FullPath
		{
			get
			{
				if(string.IsNullOrEmpty(_fullPath))
				{
					if(_parent == null)
					{
						_fullPath = _name;
					}
					else
					{
						Stack<string> paths = new Stack<string>();
						paths.Push(_name);

						var node = _parent;

						while(node != null)
						{
							paths.Push(node.Name);
							node = node.Parent;
						}

						StringBuilder text = new StringBuilder();

						while(paths.Count > 0)
						{
							text.Append(paths.Pop());

							if(paths.Count > 0)
								text.Append("/");
						}

						_fullPath = text.ToString();
					}
				}

				return _fullPath;
			}
		}

		[DefaultValue(false)]
		public bool Selected
		{
			get
			{
				return _selected;
			}
			set
			{
				_selected = value;
			}
		}

		[DefaultValue(true)]
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				_visible = value;
			}
		}

		public TreeViewNode Parent
		{
			get
			{
				return _parent;
			}
			internal set
			{
				if(object.ReferenceEquals(_parent, value))
					return;

				_parent = value;
				_fullPath = string.Empty;
			}
		}

		public bool HasNodes
		{
			get
			{
				return _nodes != null && _nodes.Count > 0;
			}
		}

		public TreeViewNodeCollection Nodes
		{
			get
			{
				if(_nodes == null)
					System.Threading.Interlocked.CompareExchange(ref _nodes, new TreeViewNodeCollection(_treeView, this), null);

				return _nodes;
			}
		}
		#endregion
	}
}

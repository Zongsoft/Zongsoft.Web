/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.ComponentModel;
using System.Reflection;

namespace Zongsoft.Web.Controls
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true)]
	public class PropertyMetadataAttribute : Attribute
	{
		#region 成员字段
		private string _attributeName;
		private bool _renderable;
		private bool _bindable;
		private object _propertyRender;
		#endregion

		#region 构造函数
		public PropertyMetadataAttribute()
		{
			_attributeName = null;
			_renderable = true;
			_bindable = true;
		}

		public PropertyMetadataAttribute(bool renderable)
		{
			_renderable = false;
			_bindable = true;
		}

		public PropertyMetadataAttribute(string attributeName)
		{
			_bindable = true;

			if(string.IsNullOrWhiteSpace(attributeName))
			{
				_attributeName = null;
				_renderable = false;
			}
			else
			{
				_attributeName = attributeName.Trim();
				_renderable = true;
			}
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取或设置控件属性对应生成的HTML特性(attribute)名。
		/// </summary>
		public string AttributeName
		{
			get
			{
				return _attributeName;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();

				_attributeName = value.Trim();
			}
		}

		/// <summary>
		/// 获取或设置控件属性是否要生成HTML特性(attribute)。
		/// </summary>
		public bool Renderable
		{
			get
			{
				return _renderable;
			}
			set
			{
				_renderable = value;
			}
		}

		/// <summary>
		/// 获取或设置控件属性是否支持绑定表达式，默认为真(True)。
		/// </summary>
		public bool Bindable
		{
			get
			{
				return _bindable;
			}
			set
			{
				_bindable = value;
			}
		}

		/// <summary>
		/// 获取或设置属性生成器的实例类型或获取该实例的静态属性路径文本。
		/// </summary>
		/// <remarks>
		///		<para>如果指定的是<see cref="System.Type"/>类型，则表示为实例类型。</para>
		///		<para>如果指定的是<see cref="System.String"/>类型，则表示为获取实例的静态属性路径文本。该路径的格式如下：</para>
		///		<para><c>Zongsoft.Web.Controls.BooleanPropertyRender.True</c>，表示获取<see cref="BooleanPropertyRender"/>类中的<c>True</c>静态属性的值。</para>
		///		<para><c>YourNamespaces.YourPropertyRender.StaticPropertyName, YourAssembly</c>，表示在<c>YourAssembly</c>程序集中查找<c>YourPropertyRender</c>类，然后获取该类中<c>StaticPropertyName</c>静态属性的值。</para>
		/// </remarks>
		public object PropertyRender
		{
			get
			{
				return _propertyRender;
			}
			set
			{
				_propertyRender = value;
			}
		}
		#endregion

		#region 公共方法
		public IPropertyRender GetPropertyRender()
		{
			if(_propertyRender == null)
				return null;

			var type = _propertyRender as Type;

			if(type != null)
			{
				if(typeof(IPropertyRender).IsAssignableFrom(type))
					return (IPropertyRender)System.Activator.CreateInstance(type);

				return null;
			}

			var text = _propertyRender as string;

			if(text != null)
			{
				var parts = text.Split(',', '@');
				var names = parts[0].Split('.');

				if(names.Length < 2)
					return null;

				var assembly = this.GetType().Assembly;

				if(parts.Length > 1)
					assembly = Assembly.Load(parts[1]);

				if(assembly == null)
					return null;

				var typeName = string.Join(".", names, 0, names.Length - 1);

				if(names.Length == 2)
					typeName = "Zongsoft.Web.Controls." + typeName;

				type = assembly.GetType(typeName, false);

				if(type != null)
				{
					var members = type.GetMember(names[names.Length - 1], BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.GetField);

					if(members == null || members.Length < 1)
						return null;

					var member = members[0];

					if(member.MemberType == MemberTypes.Field)
						return ((FieldInfo)member).GetValue(null) as IPropertyRender;
					else if(member.MemberType == MemberTypes.Property)
						return ((PropertyInfo)member).GetValue(null) as IPropertyRender;
				}
			}

			return null;
		}
		#endregion
	}
}

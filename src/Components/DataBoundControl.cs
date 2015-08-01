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
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	[ControlBuilder(typeof(DataBoundControlBuilder))]
	public class DataBoundControl : Control, IAttributeAccessor
	{
		#region 成员变量
		private readonly IDictionary<string, PropertyMetadata> _properties;
		#endregion

		#region 构造函数
		protected DataBoundControl()
		{
			//重置客户端Id的生成方式为静态
			this.ClientIDMode = System.Web.UI.ClientIDMode.Static;

			//初始化属性集合
			_properties = new Dictionary<string, PropertyMetadata>(StringComparer.OrdinalIgnoreCase);
		}
		#endregion

		#region 公共属性
		[DefaultValue("")]
		[PropertyMetadata("class")]
		public string CssClass
		{
			get
			{
				return this.GetPropertyValue(() => this.CssClass);
			}
			set
			{
				if(value != null && value.Length > 0)
					value = Utility.ResolveCssClass(value, () => this.CssClass);

				this.SetPropertyValue(() => this.CssClass, value);
			}
		}
		#endregion

		#region 保护属性
		protected IDictionary<string, PropertyMetadata> Properties
		{
			get
			{
				return _properties;
			}
		}
		#endregion

		#region 公共方法
		public void SetAttributeValue(string attributeName, string attributeValue)
		{
			if(string.IsNullOrWhiteSpace(attributeName))
				throw new ArgumentNullException("attributeName");

			_properties[attributeName] = this.CreatePropertyMetadata(attributeName, attributeValue);
		}

		protected PropertyMetadata GetProperty(string name)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			PropertyMetadata property;

			if(_properties.TryGetValue(name, out property))
				return property;

			return null;
		}

		public T GetPropertyValue<T>(string name)
		{
			var property = this.GetProperty(name);

			if(property != null)
				return Zongsoft.Common.Convert.ConvertValue<T>(property.Value, (T)property.DefaultValue);

			property = this.CreatePropertyMetadata(name, null, true);
			_properties.Add(name.Trim(), property);
			return (T)property.DefaultValue;
		}

		public T GetPropertyValue<T>(Expression<Func<T>> expression)
		{
			System.Linq.Expressions.MemberExpression exp = null;

			switch(expression.Body.NodeType)
			{
				case ExpressionType.Convert:
					exp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
					break;
				case ExpressionType.MemberAccess:
					exp = (System.Linq.Expressions.MemberExpression)expression.Body;
					break;
			}

			if(exp == null)
				throw new ArgumentException("expression");

			return this.GetPropertyValue<T>(exp.Member.Name);
		}

		public void SetPropertyValue<T>(string name, T value)
		{
			if(string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			PropertyMetadata property;

			if(_properties.TryGetValue(name, out property))
			{
				property.Value = value;
				return;
			}

			_properties.Add(name.Trim(), this.CreatePropertyMetadata(name, value, true));
		}

		public void SetPropertyValue<T>(Expression<Func<T>> expression, T value)
		{
			System.Linq.Expressions.MemberExpression exp = null;

			switch(expression.Body.NodeType)
			{
				case ExpressionType.Convert:
					exp = ((UnaryExpression)expression.Body).Operand as MemberExpression;
					break;
				case ExpressionType.MemberAccess:
					exp = (System.Linq.Expressions.MemberExpression)expression.Body;
					break;
			}

			if(exp == null)
				throw new ArgumentException("expression");

			this.SetPropertyValue<T>(exp.Member.Name, value);
		}
		#endregion

		#region 生成方法
		protected override void Render(HtmlTextWriter writer)
		{
			this.RenderBeginTag(writer);
			this.RenderContent(writer);
			this.RenderEndTag(writer);
		}

		protected virtual void RenderBeginTag(HtmlTextWriter writer)
		{
		}

		protected virtual void RenderEndTag(HtmlTextWriter writer)
		{
		}

		protected virtual void RenderContent(HtmlTextWriter writer)
		{
			this.RenderChildren(writer);
		}

		protected virtual void AddAttributes(HtmlTextWriter writer, params string[] ignoreProperties)
		{
			if(!string.IsNullOrWhiteSpace(this.ID) && !Zongsoft.Common.StringExtension.In("ID", ignoreProperties, StringComparison.OrdinalIgnoreCase))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			foreach(var property in _properties.Values)
			{
				if(property.Renderable && !Zongsoft.Common.StringExtension.In(property.Name, ignoreProperties, StringComparison.OrdinalIgnoreCase))
					this.AddAttribute(writer, property);
			}
		}

		protected virtual void AddAttribute(HtmlTextWriter writer, PropertyMetadata property)
		{
			var propertyRender = property.PropertyRender;

			//如果指定了属性生成器，则调用属性生成器来生成结果
			if(propertyRender != null && propertyRender.RenderProperty(writer, property))
				return;

			if(!string.IsNullOrEmpty(property.AttributeValue))
				writer.AddAttribute(property.AttributeName, property.AttributeValue);
		}
		#endregion

		#region 初始方法
		protected override void OnInit(EventArgs e)
		{
			PropertyMetadata visible;

			if(_properties.TryGetValue("Visible", out visible))
			{
				this.Visible = (bool)BindingUtility.GetBindingValue(visible.ValueString, this.GetBindingSource(), typeof(bool));
				_properties.Remove("Visible");
			}

			//调用基类同名方法
			base.OnInit(e);
		}
		#endregion

		#region 显式实现
		string IAttributeAccessor.GetAttribute(string key)
		{
			return _properties[key.Trim('$')].ValueString;
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			key = key.Trim('$');

			//注意：以下对 ID 的特殊处理是为了解决 Mono in Linux 中的系统错误
			if(string.Equals(key, "ID", StringComparison.OrdinalIgnoreCase))
			{
				this.ID = value;
				return;
			}

			_properties[key] = this.CreatePropertyMetadata(key, value);
		}
		#endregion

		#region 保护方法
		internal protected object GetBindingSource()
		{
			return BindingUtility.GetBindingSource(this);
		}
		#endregion

		#region 私有方法
		private PropertyMetadata CreatePropertyMetadata(string propertyName, string valueString)
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			PropertyMetadata propertyMetadata = null;
			var property = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

			if(property == null)
				propertyMetadata = new PropertyMetadata(this, propertyName, valueString);
			else
			{
				propertyMetadata = this.CreatePropertyMetadata(property, null);
				propertyMetadata.ValueString = valueString;
			}

			return propertyMetadata;
		}

		private PropertyMetadata CreatePropertyMetadata(string propertyName, object value, bool throwsNotExists)
		{
			if(string.IsNullOrWhiteSpace(propertyName))
				throw new ArgumentNullException("propertyName");

			var property = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);

			if(property == null)
			{
				if(throwsNotExists)
					throw new InvalidOperationException(string.Format("Not found the '{0}' property of '{1}' control.", propertyName, this.GetType().FullName));
				else
					return null;
			}

			return this.CreatePropertyMetadata(property, value);
		}

		private PropertyMetadata CreatePropertyMetadata(PropertyInfo propertyInfo, object value)
		{
			if(propertyInfo == null)
				throw new ArgumentNullException("propertyInfo");

			var propertyMetadata = new PropertyMetadata(this, propertyInfo.Name, propertyInfo.PropertyType, value);

			var attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(DefaultValueAttribute), true);
			if(attribute != null)
				propertyMetadata.DefaultValue = ((DefaultValueAttribute)attribute).Value;

			attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(BindableAttribute), true);
			if(attribute != null)
				propertyMetadata.Bindable = ((BindableAttribute)attribute).Bindable;

			attribute = Attribute.GetCustomAttribute(propertyInfo, typeof(PropertyMetadataAttribute), true);
			if(attribute != null)
			{
				var attributeName = ((PropertyMetadataAttribute)attribute).AttributeName;

				if(!string.IsNullOrWhiteSpace(attributeName))
					propertyMetadata.AttributeName = attributeName;

				propertyMetadata.Renderable = ((PropertyMetadataAttribute)attribute).Renderable;
				propertyMetadata.Bindable = ((PropertyMetadataAttribute)attribute).Bindable;
				propertyMetadata.PropertyRender = ((PropertyMetadataAttribute)attribute).GetPropertyRender();
			}

			return propertyMetadata;
		}
		#endregion
	}
}

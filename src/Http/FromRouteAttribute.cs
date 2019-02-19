/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2017 Zongsoft Corporation <http://www.zongsoft.com>
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
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
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
using System.Linq;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Metadata;
using System.Web.Http.Controllers;

namespace Zongsoft.Web.Http
{
	[AttributeUsage(AttributeTargets.Parameter)]
	public class FromRouteAttribute : ParameterBindingAttribute
	{
		#region 成员字段
		private string _key;
		private int _ordinal;
		private Type _converterType;
		#endregion

		#region 构造函数
		public FromRouteAttribute(string key)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			_key = key.Trim();
			_ordinal = -1;
		}

		public FromRouteAttribute(string key, int ordinal)
		{
			if(string.IsNullOrWhiteSpace(key))
				throw new ArgumentNullException(nameof(key));

			_key = key.Trim();
			_ordinal = ordinal;
		}
		#endregion

		#region 公共属性
		/// <summary>
		/// 获取绑定到的路由项键名称，譬如：id、args 等。
		/// </summary>
		public string Key
		{
			get
			{
				return _key;
			}
		}

		/// <summary>
		/// 获取或设置绑定到相同路由项被分隔后的条目序号。默认值-1，表示按参数声明的顺序绑定。
		/// </summary>
		public int Ordinal
		{
			get
			{
				return _ordinal;
			}
			set
			{
				_ordinal = value;
			}
		}

		/// <summary>
		/// 获取或设置绑定参数值的类型转换器的类型。
		/// </summary>
		public Type ConverterType
		{
			get
			{
				return _converterType;
			}
			set
			{
				if(value != null && !typeof(System.ComponentModel.TypeConverter).IsAssignableFrom(value))
					throw new ArgumentException("Invalid converter type.");

				_converterType = value;
			}
		}
		#endregion

		#region 重写方法
		public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
		{
			return new FromRouteBinding(parameter);
		}
		#endregion

		#region 嵌套子类
		private class FromRouteBinding : HttpParameterBinding
		{
			#region 构造函数
			public FromRouteBinding(HttpParameterDescriptor descriptor) : base(descriptor)
			{
			}
			#endregion

			#region 公共属性
			public FromRouteAttribute Attribute
			{
				get
				{
					return (FromRouteAttribute)this.Descriptor.ParameterBinderAttribute;
				}
			}
			#endregion

			#region 重写方法
			public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
			{
				var routeData = actionContext.RequestContext.RouteData;
				object value;

				//从当前请求的路由数据中获取指定的键值，获取失败则抛出异常
				if(!routeData.Values.TryGetValue(this.Attribute.Key, out value))
					return TaskHelper.FromError(HttpResponseExceptionUtility.BadRequest(string.Format("Resolve '{0}' route data failed for the '{1}' parameter.", this.Attribute.Key, this.Descriptor.ParameterName)));

				//获取指定键值切分之后的部位顺序
				var ordinal = this.GetOrdinal(actionContext);

				//如果获取的切分部位顺序为正数则必须进行切片处理
				if(ordinal >= 0 && value != null)
				{
					var parts = Utility.Slice(value.ToString());

					if(ordinal < parts.Length)
						value = parts[ordinal];
					else
						value = this.Descriptor.DefaultValue;
				}

				//进行类型转换处理
				if(this.Attribute.ConverterType == null)
				{
					object targetValue;

					if(value != null && value is string text && text.Length > 0 && (this.Descriptor.ParameterType.IsArray || Zongsoft.Common.TypeExtension.IsCollection(this.Descriptor.ParameterType)))
					{
						var parts = text.Split(',');
						var elementType = Zongsoft.Common.TypeExtension.GetElementType(this.Descriptor.ParameterType);
						var array = Array.CreateInstance(elementType, parts.Length);

						for(int i = 0; i < parts.Length; i++)
						{
							if(Zongsoft.Common.Convert.TryConvertValue(parts[i], elementType, out var element))
								array.SetValue(element, i);
							else
								actionContext.ModelState.AddModelError(this.Descriptor.ParameterName, $"The specified '{parts[i]}' is an invalid element value and cannot be converted to '{elementType.Name}' type.");
						}

						value = array;
					}
					else if(Zongsoft.Common.Convert.TryConvertValue(value, this.Descriptor.ParameterType, out targetValue))
						value = targetValue;
					else
						value = this.Descriptor.DefaultValue;
				}
				else
				{
					var converter = (TypeConverter)System.Activator.CreateInstance(this.Attribute.ConverterType);

					if(converter.CanConvertTo(this.Descriptor.ParameterType))
						value = converter.ConvertTo(value, this.Descriptor.ParameterType);
				}

				return Task.Run(() => this.SetValue(actionContext, value));
			}
			#endregion

			#region 私有方法
			private int GetOrdinal(HttpActionContext actionContext)
			{
				return (int)actionContext.ActionDescriptor.Properties.GetOrAdd(this.GetCacheKey(), _ => this.GenerateOrdinal(actionContext));
			}

			private int GenerateOrdinal(HttpActionContext actionContext)
			{
				var parameters = actionContext.ActionDescriptor.GetParameters().Where(p =>
				{
					var attribute = p.ParameterBinderAttribute as FromRouteAttribute;
					return attribute != null && string.Equals(this.Attribute.Key, attribute.Key, StringComparison.OrdinalIgnoreCase);
				});

				if(parameters.Count() > 1)
				{
					var index = 0;

					foreach(var parameter in parameters)
					{
						var attribute = (FromRouteAttribute)parameter.ParameterBinderAttribute;

						if(parameter.ParameterName == this.Descriptor.ParameterName)
							return index;

						if(attribute.Ordinal < 0)
							index++;
					}
				}

				return -1;
			}

			private string GetCacheKey()
			{
				return this.Descriptor.ParameterName + ":Ordinal";
			}
			#endregion
		}
		#endregion
	}
}

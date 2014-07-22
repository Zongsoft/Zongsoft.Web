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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class PagingBar : DataBoundControl
	{
		#region 构造函数
		public PagingBar()
		{
			this.FirstPageTitle = "|<";
			this.PreviousPageTitle = "<";
			this.NextPageTitle = ">";
			this.LastPageTitle = ">|";
		}
		#endregion

		#region 公共属性
		[Bindable(true)]
		[DefaultValue(0)]
		public int PageCount
		{
			get
			{
				return this.GetAttributeValue<int>("PageCount", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.PageCount, Math.Max(value, 0));
			}
		}

		[Bindable(true)]
		[DefaultValue(20)]
		public int PageSize
		{
			get
			{
				return this.GetAttributeValue<int>("PageSize", 20);
			}
			set
			{
				this.SetAttributeValue(() => this.PageSize, Math.Max(value, 1));
			}
		}

		[Bindable(true)]
		[DefaultValue(0)]
		public int PageIndex
		{
			get
			{
				return this.GetAttributeValue<int>("PageIndex", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.PageIndex, Math.Max(value, 0));
			}
		}

		[Bindable(true)]
		[DefaultValue(0)]
		public int TotalCount
		{
			get
			{
				return this.GetAttributeValue<int>("TotalCount", 0);
			}
			set
			{
				this.SetAttributeValue(() => this.TotalCount, Math.Max(value, 0));
			}
		}

		[Bindable(true)]
		public string Url
		{
			get
			{
				return this.GetAttributeValue<string>("Url", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.Url, value);
			}
		}

		[Bindable(true)]
		public string FirstPageTitle
		{
			get
			{
				return this.GetAttributeValue<string>("FirstPageTitle", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.FirstPageTitle, value);
			}
		}

		[Bindable(true)]
		public string PreviousPageTitle
		{
			get
			{
				return this.GetAttributeValue<string>("PreviousPageTitle", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.PreviousPageTitle, value);
			}
		}

		[Bindable(true)]
		public string NextPageTitle
		{
			get
			{
				return this.GetAttributeValue<string>("NextPageTitle", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.NextPageTitle, value);
			}
		}

		[Bindable(true)]
		public string LastPageTitle
		{
			get
			{
				return this.GetAttributeValue<string>("LastPageTitle", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.LastPageTitle, value);
			}
		}

		[Bindable(true)]
		public string MorePageTitle
		{
			get
			{
				return this.GetAttributeValue<string>("MorePageTitle", string.Empty);
			}
			set
			{
				this.SetAttributeValue(() => this.MorePageTitle, value);
			}
		}

		/// <summary>
		/// 获取或设置
		/// </summary>
		[Bindable(true)]
		[DefaultValue("")]
		public string PageNumberFormat
		{
			get
			{
				var result = this.GetAttributeValue<string>("PageNumberFormat", string.Empty);

				if(string.IsNullOrWhiteSpace(result))
					return "{0}/{1}";

				return result;
			}
			set
			{
				this.SetAttributeValue(() => this.PageNumberFormat, value);
			}
		}

		[Bindable(true)]
		[DefaultValue("")]
		public string TotalCountFormat
		{
			get
			{
				var result = this.GetAttributeValue<string>("TotalCountFormat", string.Empty);

				if(string.IsNullOrWhiteSpace(result))
					return "{0}";

				return result;
			}
			set
			{
				this.SetAttributeValue(() => this.TotalCountFormat, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
		public bool TotalCountVisible
		{
			get
			{
				return this.GetAttributeValue<bool>("TotalCountVisible", false);
			}
			set
			{
				this.SetAttributeValue(() => this.TotalCountVisible, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(false)]
		public bool RedirectPageVisible
		{
			get
			{
				return this.GetAttributeValue<bool>("RedirectPageVisible", false);
			}
			set
			{
				this.SetAttributeValue(() => this.RedirectPageVisible, value);
			}
		}
		#endregion

		#region 公共方法
		public string GetUrl(int pageIndex)
		{
			var result = this.Url;

			if(result != null)
			{
				result = result.Replace("{PageIndex}", pageIndex.ToString());
				result = result.Replace("{PageCount}", this.PageCount.ToString());
				result = result.Replace("{PageSize}", this.PageSize.ToString());
			}

			return result ?? string.Empty;
		}
		#endregion

		#region 生成控件
		public override void RenderControl(HtmlTextWriter writer)
		{
			var range = Zongsoft.Common.PagingEvaluator.Instance.Evaluate(this.PageIndex, this.PageCount, this.PageSize);

			if(this.PageCount < 1)
				return;

			if(!string.IsNullOrWhiteSpace(this.ID))
				writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID);

			if(string.IsNullOrWhiteSpace(this.CssClass))
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "pagingBar");
			else
				writer.AddAttribute(HtmlTextWriterAttribute.Class, this.CssClass);

			//生成分页器的容器标记(开始)
			writer.RenderBeginTag(HtmlTextWriterTag.Div);

			//生成“{PageIndex}/{PageCount}”页码信息
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "pageNumber");
			writer.RenderBeginTag(HtmlTextWriterTag.Span);
			writer.WriteEncodedText(string.Format(this.PageNumberFormat, this.PageIndex, this.PageCount));
			writer.RenderEndTag();

			if(this.TotalCountVisible)
			{
				//生成“{TotalCount}”总记录数
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "totalCount");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				writer.WriteEncodedText(string.Format(this.TotalCountFormat, this.TotalCount));
				writer.RenderEndTag();
			}

			//生成“第一页”的导航元素
			writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetUrl(1));
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(this.FirstPageTitle);
			writer.RenderEndTag();

			//生成“上一页”的导航元素
			writer.AddAttribute(HtmlTextWriterAttribute.Href, (this.PageIndex > 1 ? this.GetUrl(this.PageIndex - 1) : Utility.EmptyLink));
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(this.PreviousPageTitle);
			writer.RenderEndTag();

			if(range.StartIndex > 1 && !string.IsNullOrWhiteSpace(this.MorePageTitle))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "more");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				writer.Write(this.MorePageTitle);
				writer.RenderEndTag();
			}

			//生成导航元素
			for(int i = range.StartIndex; i <= range.FinishIndex; i++)
			{
				if(i == this.PageIndex)
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Class, "current");
					writer.RenderBeginTag(HtmlTextWriterTag.Span);
					writer.Write(i);
					writer.RenderEndTag();
				}
				else
				{
					writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetUrl(i));
					writer.RenderBeginTag(HtmlTextWriterTag.A);
					writer.Write(i);
					writer.RenderEndTag();
				}
			}

			if(range.FinishIndex < this.PageCount && !string.IsNullOrWhiteSpace(this.MorePageTitle))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "more");
				writer.RenderBeginTag(HtmlTextWriterTag.Span);
				writer.Write(this.MorePageTitle);
				writer.RenderEndTag();
			}

			//生成“下一页”的导航元素
			writer.AddAttribute(HtmlTextWriterAttribute.Href, (this.PageIndex < this.PageCount ? this.GetUrl(this.PageIndex + 1) : Utility.EmptyLink));
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(this.NextPageTitle);
			writer.RenderEndTag();

			//生成“最后页”的导航元素
			writer.AddAttribute(HtmlTextWriterAttribute.Href, this.GetUrl(this.PageCount));
			writer.RenderBeginTag(HtmlTextWriterTag.A);
			writer.WriteEncodedText(this.LastPageTitle);
			writer.RenderEndTag();

			if(this.RedirectPageVisible)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
				writer.AddAttribute(HtmlTextWriterAttribute.Class, "number");
				writer.RenderBeginTag(HtmlTextWriterTag.Input);
				writer.RenderEndTag();

				writer.AddAttribute(HtmlTextWriterAttribute.Class, "go");
				writer.AddAttribute(HtmlTextWriterAttribute.Href, Utility.EmptyLink);
				writer.RenderBeginTag(HtmlTextWriterTag.A);
				writer.Write("Go");
				writer.RenderEndTag();
			}

			//生成分页器的容器标记(结束)
			writer.RenderEndTag();
		}
		#endregion
	}
}

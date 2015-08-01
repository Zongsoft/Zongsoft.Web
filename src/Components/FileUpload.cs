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
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Zongsoft.Web.Controls
{
	public class FileUpload : InputBox
	{
		#region 构造函数
		public FileUpload() : base(InputBoxType.File)
		{
		}
		#endregion

		#region 重写属性
		[Browsable(false)]
		[DefaultValue(InputBoxType.File)]
		public override InputBoxType InputType
		{
			get
			{
				return base.InputType;
			}
			set
			{
				if(value != InputBoxType.File)
					throw new ArgumentOutOfRangeException();

				base.InputType = value;
			}
		}
		#endregion

		#region 重写方法
		protected override void Render(HtmlTextWriter writer)
		{
			this.AddAttributes(writer);

			writer.AddAttribute(HtmlTextWriterAttribute.Style, "display:none");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			if(!string.IsNullOrWhiteSpace(this.Label))
			{
				writer.AddAttribute(HtmlTextWriterAttribute.For, this.ID + "_input");
				writer.RenderBeginTag(HtmlTextWriterTag.Label);
				writer.WriteEncodedText(this.Label);
				writer.RenderEndTag();
			}

			writer.AddAttribute(HtmlTextWriterAttribute.Id, this.ID + "_input");
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.ID + "_input");
			writer.AddAttribute(HtmlTextWriterAttribute.Type, "text");
			writer.AddAttribute(HtmlTextWriterAttribute.Class, "file");
			writer.AddAttribute(HtmlTextWriterAttribute.Value, this.Value);

			if(!this.Enabled)
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");

			writer.RenderBeginTag(HtmlTextWriterTag.Input);
			writer.RenderEndTag();

			//调用基类同名方法
			base.Render(writer);
		}
		#endregion
	}
}

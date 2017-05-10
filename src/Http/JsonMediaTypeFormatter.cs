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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;

namespace Zongsoft.Web.Http
{
	public class JsonMediaTypeFormatter : System.Net.Http.Formatting.MediaTypeFormatter
	{
		#region 静态常量
		private static readonly System.Net.Http.Headers.MediaTypeHeaderValue ApplicationJsonMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
		private static readonly System.Net.Http.Headers.MediaTypeHeaderValue TextJsonMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/json");
		#endregion

		#region 构造函数
		public JsonMediaTypeFormatter()
		{
			this.SupportedEncodings.Add(new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
			this.SupportedEncodings.Add(new System.Text.UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

			this.SupportedMediaTypes.Add(ApplicationJsonMediaType);
			this.SupportedMediaTypes.Add(TextJsonMediaType);

			this.MediaTypeMappings.Add(new XmlHttpRequestHeaderMapping());
		}
		#endregion

		#region 重写方法
		public override bool CanReadType(Type type)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));

			return true;
		}

		public override bool CanWriteType(Type type)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));

			return true;
		}

		public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			if(readStream == null)
				throw new ArgumentNullException(nameof(readStream));

			try
			{
				return Task.FromResult(this.ReadFromStream(type, readStream, content, formatterLogger));
			}
			catch(Exception ex)
			{
				return TaskHelper.FromError<object>(ex);
			}
		}

		private object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
		{
			HttpContentHeaders contentHeaders = content == null ? null : content.Headers;

			if(contentHeaders != null && contentHeaders.ContentLength == 0)
				return GetDefaultValueForType(type);

			var effectiveEncoding = this.SelectCharacterEncoding(contentHeaders);

			try
			{
				return Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize(readStream, type);
			}
			catch(Exception ex)
			{
				if(formatterLogger == null)
					throw;

				formatterLogger.LogError(String.Empty, ex);
				return GetDefaultValueForType(type);
			}
		}

		public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext, CancellationToken cancellationToken)
		{
			if(type == null)
				throw new ArgumentNullException(nameof(type));
			if(writeStream == null)
				throw new ArgumentNullException(nameof(writeStream));

			if(cancellationToken.IsCancellationRequested)
				return TaskHelper.Canceled();

			try
			{
				this.WriteToStream(type, value, writeStream, content);
				return TaskHelper.Completed();
			}
			catch(Exception ex)
			{
				return TaskHelper.FromError(ex);
			}
		}

		private void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
		{
			var effectiveEncoding = this.SelectCharacterEncoding(content == null ? null : content.Headers);
			Zongsoft.Runtime.Serialization.Serializer.Json.Serialize(writeStream, value);
		}
		#endregion
	}
}

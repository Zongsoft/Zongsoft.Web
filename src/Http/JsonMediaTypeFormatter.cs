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
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Zongsoft.Runtime.Serialization;

namespace Zongsoft.Web.Http
{
	public class JsonMediaTypeFormatter : System.Net.Http.Formatting.MediaTypeFormatter
	{
		#region 静态常量
		private static readonly System.Net.Http.Headers.MediaTypeHeaderValue ApplicationJsonMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
		private static readonly System.Net.Http.Headers.MediaTypeHeaderValue TextJsonMediaType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/json");
		#endregion

		#region 私有变量
		private Runtime.Serialization.TextSerializationSettings _settings;
		#endregion

		#region 构造函数
		public JsonMediaTypeFormatter() : this(null)
		{
		}

		public JsonMediaTypeFormatter(Zongsoft.Runtime.Serialization.TextSerializationSettings settings)
		{
			_settings = settings;

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

			using(var writer = new StreamWriter(writeStream, effectiveEncoding))
			{
				Runtime.Serialization.Serializer.Json.Serialize(writer, value, _settings);
			}
		}

		public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
		{
			var settings = this.GetSerializationSettings(request.Headers);

			if(settings == null)
				return base.GetPerRequestFormatterInstance(type, request, mediaType);
			else
				return new JsonMediaTypeFormatter(settings);
		}
		#endregion

		#region 私有方法
		private TextSerializationSettings GetSerializationSettings(HttpRequestHeaders headers)
		{
			if(headers == null)
				return null;

			var behaviors = this.GetHeaderValues(headers, "x-json-behaviors");
			var datetimeFormat = this.GetHeaderValue(headers, "x-json-datetime");
			var casing = this.GetHeaderValue(headers, "x-json-casing");

			if((behaviors == null || behaviors.Count == 0) && string.IsNullOrEmpty(datetimeFormat) && string.IsNullOrEmpty(casing))
				return null;

			var settings = new TextSerializationSettings()
			{
				DateTimeFormat = datetimeFormat
			};

			if(!string.IsNullOrEmpty(casing) && Enum.TryParse<SerializationNamingConvention>(casing, true, out var naming))
				settings.NamingConvention = naming;

			if(behaviors != null && behaviors.Count > 0)
			{
				settings.Indented = behaviors.Contains("indented");

				if(behaviors.Contains("ignores:none"))
					settings.SerializationBehavior = SerializationBehavior.None;
				else if(behaviors.Contains("ignores:default") || behaviors.Contains("ignores:null"))
					settings.SerializationBehavior = SerializationBehavior.IgnoreDefaultValue;
			}

			return settings;
		}

		private ISet<string> GetHeaderValues(HttpHeaders headers, string name)
		{
			if(headers == null || string.IsNullOrEmpty(name))
				return null;

			IEnumerable<string> values;

			if(headers.TryGetValues(name, out values) && values != null)
			{
				var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

				foreach(var value in values)
				{
					if(string.IsNullOrWhiteSpace(value))
						continue;

					var parts = value.Split(',', ';').Where(p => !string.IsNullOrWhiteSpace(p)).Select(p => p.Trim());
					hashset.UnionWith(parts);
				}

				if(hashset.Count > 0)
					return hashset;
			}

			return null;
		}

		private string GetHeaderValue(HttpHeaders headers, string name)
		{
			if(headers == null || string.IsNullOrEmpty(name))
				return null;

			IEnumerable<string> values;

			if(headers.TryGetValues(name, out values))
				return string.Join(string.Empty, values);

			return null;
		}
		#endregion
	}
}

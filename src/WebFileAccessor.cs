/*
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@gmail.com>
 *
 * Copyright (C) 2015-2016 Zongsoft Corporation <http://www.zongsoft.com>
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
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Net.Http.Headers;

using Zongsoft.IO;
using Zongsoft.Collections;

namespace Zongsoft.Web
{
	public class WebFileAccessor
	{
		#region 常量定义
		private const string EXTENDED_PROPERTY_PREFIX = "x-zfs-";

		private const string EXTENDED_PROPERTY_DISPOSITIONNAME = "DispositionName";
		private const string EXTENDED_PROPERTY_FILENAME = "FileName";
		#endregion

		#region 成员字段
		private string _basePath;
		#endregion

		#region 构造函数
		public WebFileAccessor()
		{
		}

		public WebFileAccessor(string basePath)
		{
			_basePath = basePath;
		}
		#endregion

		#region 公共属性
		public string BasePath
		{
			get
			{
				return _basePath;
			}
			set
			{
				if(string.IsNullOrWhiteSpace(value))
					_basePath = null;
				else
				{
					var text = value.Trim();
					_basePath = text + (text.EndsWith("/") ? string.Empty : "/");
				}
			}
		}
		#endregion

		#region 公共方法
		/// <summary>
		/// 下载指定路径的文件。
		/// </summary>
		/// <param name="path">指定要下载的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public HttpResponseMessage Read(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			var stream = FileSystem.File.Open(this.GetFilePath(path), FileMode.Open, FileAccess.Read, properties);

			if(stream == null)
				throw new HttpResponseException(HttpStatusCode.NotFound);

			//创建当前文件的流内容
			var content = new StreamContent(stream);

			//设置返回的内容头信息
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
			content.Headers.ContentType = new MediaTypeHeaderValue(System.Web.MimeMapping.GetMimeMapping(path));

			if(properties.Count > 0)
			{
				string fileName, lastModified;

				if(properties.TryGetValue("FileName", out fileName))
					content.Headers.ContentDisposition.FileName = fileName;

				if(properties.TryGetValue("HTTP:LastModified", out lastModified))
				{
					DateTimeOffset modifiedTime;

					if(Zongsoft.Common.Convert.TryConvertValue<DateTimeOffset>(lastModified, out modifiedTime))
						content.Headers.LastModified = modifiedTime;
				}
			}

			return new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = content,
			};
		}

		/// <summary>
		/// 获取指定文件的外部访问路径。
		/// </summary>
		/// <param name="path">指定的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回指定文件的外部访问路径。</returns>
		public string GetUrl(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return FileSystem.GetUrl(this.GetFilePath(path));
		}

		/// <summary>
		/// 获取指定路径的文件描述信息。
		/// </summary>
		/// <param name="path">指定要获取的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		/// <returns>返回的指定的文件详细信息。</returns>
		public Task<Zongsoft.IO.FileInfo> GetInfo(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return FileSystem.File.GetInfoAsync(this.GetFilePath(path));
		}

		/// <summary>
		/// 删除指定相对路径的文件。
		/// </summary>
		/// <param name="path">指定要删除的文件的相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public async Task<bool> Delete(string path)
		{
			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			return await FileSystem.File.DeleteAsync(this.GetFilePath(path));
		}

		/// <summary>
		/// 修改指定路径的文件描述信息。
		/// </summary>
		/// <param name="request">网络请求消息。</param>
		/// <param name="path">指定要修改的文件相对路径或绝对路径（绝对路径以/斜杠打头）。</param>
		public async Task<bool> SetInfo(HttpRequestMessage request, string path)
		{
			if(request == null)
				throw new ArgumentNullException("request");

			if(string.IsNullOrWhiteSpace(path))
				throw new ArgumentNullException("path");

			var properties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

			foreach(var header in request.Headers)
			{
				if(header.Key.Length > EXTENDED_PROPERTY_PREFIX.Length && header.Key.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
					properties[header.Key.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = string.Join("", header.Value);
			}

			if(request.Content.IsFormData())
			{
				var form = await request.Content.ReadAsFormDataAsync();

				foreach(string fieldName in form)
				{
					if(fieldName.Length > EXTENDED_PROPERTY_PREFIX.Length && fieldName.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
						properties[fieldName.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = form[fieldName];
				}
			}

			if(properties.Count > 0)
				return await FileSystem.File.SetInfoAsync(this.GetFilePath(path), properties);

			return false;
		}

		/// <summary>
		/// 将网络请求中的一个文件或多个文件写入到指定的目录中。
		/// </summary>
		/// <param name="request">网络请求消息。</param>
		/// <param name="directory">指定文件写入的目录路径（绝对路径以“/”斜杠符打头）；如果为空(null)或全空字符串，则写入目录为<see cref="BasePath"/>属性指定的路径。</param>
		/// <param name="onWriting">当文件写入前激发的通知回调。</param>
		/// <returns>返回写入成功的<see cref="Zongsoft.IO.FileInfo"/>文件描述信息实体对象集。</returns>
		public async Task<IEnumerable<Zongsoft.IO.FileInfo>> Write(HttpRequestMessage request, string directory = null, Action<WritingEventArgs> onWriting = null)
		{
			var filePath = this.GetFilePath(directory);

			//检测请求的内容是否为Multipart类型
			if(!request.Content.IsMimeMultipartContent("form-data"))
				throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

			//创建自定义头的字典
			var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			//构建自定义头的字典内容
			foreach(var header in request.Headers)
			{
				if(header.Key.Length > EXTENDED_PROPERTY_PREFIX.Length && header.Key.StartsWith(EXTENDED_PROPERTY_PREFIX, StringComparison.OrdinalIgnoreCase))
					headers[header.Key.Substring(EXTENDED_PROPERTY_PREFIX.Length)] = string.Join("", header.Value);
			}

			//创建多段表单信息的文件流操作的供应程序
			var provider = new MultipartStorageFileStreamProvider(filePath, headers, onWriting);

			//从当前请求内容读取多段信息并写入文件中
			var result = await request.Content.ReadAsMultipartAsync(provider);

			if(result.FormData != null && result.FormData.Count > 0)
			{
				foreach(var fileInfo in result.FileData)
				{
					object dispositionName;
					string prefix;

					if(fileInfo.HasProperties && fileInfo.Properties.TryGetValue(EXTENDED_PROPERTY_DISPOSITIONNAME, out dispositionName))
						prefix = EXTENDED_PROPERTY_PREFIX + (string)dispositionName + "-";
					else
						continue;

					var updateRequires = false;

					foreach(var formEntry in result.FormData)
					{
						if(formEntry.Key.Length > prefix.Length && formEntry.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
						{
							updateRequires = true;
							fileInfo.Properties[formEntry.Key.Substring(prefix.Length)] = formEntry.Value;
						}
					}

					if(updateRequires)
						await FileSystem.File.SetInfoAsync(fileInfo.Path.Url, fileInfo.Properties);
				}
			}

			//返回新增的文件信息实体集
			return result.FileData;
		}
		#endregion

		#region 私有方法
		private string EnsureBasePath(out string scheme)
		{
			string path;

			if(Zongsoft.IO.Path.TryParse(this.BasePath, out scheme, out path))
				return (scheme ?? Zongsoft.IO.FileSystem.Scheme) + ":" + (path ?? "/");

			scheme = Zongsoft.IO.FileSystem.Scheme;
			return scheme + ":/";
		}

		private string GetFilePath(string path)
		{
			string scheme;
			string basePath = this.EnsureBasePath(out scheme);

			if(string.IsNullOrWhiteSpace(path))
				return basePath;

			path = Uri.UnescapeDataString(path).Trim();

			if(path.StartsWith("/"))
				return scheme + ":" + path;
			else
				return Zongsoft.IO.Path.Combine(basePath, path);
		}
		#endregion

		#region 嵌套子类
		public class WritingEventArgs : EventArgs
		{
			#region 成员字段
			private int _index;
			private bool _cancel;
			private bool _overwrite;
			private bool _extensionAppend;
			private string _directory;
			private string _fileName;
			#endregion

			#region 构造函数
			public WritingEventArgs(string directory, string fileName, int index)
			{
				_index = index;
				_cancel = false;
				_overwrite = false;
				_extensionAppend = true;
				_directory = directory;
				_fileName = fileName;
			}
			#endregion

			#region 公共属性
			/// <summary>
			/// 获取当前文件的序号，从一开始。
			/// </summary>
			public int Index
			{
				get
				{
					return _index;
				}
			}

			/// <summary>
			/// 获取或设置一个值，指示是否取消后续的文件写入操作。
			/// </summary>
			public bool Cancel
			{
				get
				{
					return _cancel;
				}
				set
				{
					_cancel = value;
				}
			}

			/// <summary>
			/// 获取或设置一个值，指示当前文件操作是否为覆盖写入的方式。
			/// </summary>
			public bool Overwrite
			{
				get
				{
					return _overwrite;
				}
				set
				{
					_overwrite = value;
				}
			}

			/// <summary>
			/// 获取或设置一个值，指示是否自动添加文件的扩展名。
			/// </summary>
			public bool ExtensionAppend
			{
				get
				{
					return _extensionAppend;
				}
				set
				{
					_extensionAppend = value;
				}
			}

			/// <summary>
			/// 获取当前要写入文件所在的目录地址。
			/// </summary>
			public string Directory
			{
				get
				{
					return _directory;
				}
			}

			/// <summary>
			/// 获取或设置要写入的文件名，如果未包含扩展名则使用上传文件的原始扩展名。
			/// </summary>
			public string FileName
			{
				get
				{
					return _fileName;
				}
				set
				{
					_fileName = value;
				}
			}
			#endregion
		}

		private class MultipartStorageFileStreamProvider : MultipartStreamProvider
		{
			#region 常量定义
			//注意：扩展属性名包含冒号(:)的会被某些文件系统忽略
			private const string EXTENDED_PROPERTY_FILESTREAM = "ignore:FileStream";

			private static readonly DateTime EPOCH = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			#endregion

			#region 成员字段
			private int _index;
			private string _filePath;
			private string _directory;
			private IDictionary<string, string> _headers;
			private IList<Zongsoft.IO.FileInfo> _fileData;
			private IDictionary<string, string> _formData;
			private System.Collections.ObjectModel.Collection<bool> _isFormData;
			private Action<WritingEventArgs> _onWriting;
			#endregion

			#region 构造函数
			public MultipartStorageFileStreamProvider(string filePath, IDictionary<string, string> headers, Action<WritingEventArgs> onWriting)
			{
				if(string.IsNullOrEmpty(filePath))
					throw new ArgumentNullException(nameof(filePath));

				_filePath = filePath;
				_headers = headers;
				_fileData = new List<Zongsoft.IO.FileInfo>();
				_isFormData = new System.Collections.ObjectModel.Collection<bool>();
				_formData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				_onWriting = onWriting;

				if(_filePath[_filePath.Length - 1] == '/')
					_directory = _filePath;
				else
				{
					var index = _filePath.LastIndexOf('/');

					if(index > 0)
						_directory = _filePath.Substring(0, index + 1);
				}
			}
			#endregion

			#region 公共属性
			public string FilePath
			{
				get
				{
					return _filePath;
				}
			}

			public IList<Zongsoft.IO.FileInfo> FileData
			{
				get
				{
					return _fileData;
				}
			}

			public IDictionary<string, string> FormData
			{
				get
				{
					return _formData;
				}
			}
			#endregion

			#region 重写方法
			public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
			{
				if(parent == null)
					throw new ArgumentNullException(nameof(parent));
				if(headers == null)
					throw new ArgumentNullException(nameof(headers));

				//判断当前内容项是否为普通表单域，如果是则返回一个内存流
				if(headers.ContentDisposition == null || string.IsNullOrEmpty(headers.ContentDisposition.FileName))
				{
					//在表单数据标记列表中按顺序将当前内容标记为普通表单域
					_isFormData.Add(true);

					//返回一个新的内存流
					return new MemoryStream();
				}

				//获取当前内容项的内容类型
				var contentType = headers.ContentType == null ? string.Empty : headers.ContentType.MediaType;

				if(string.IsNullOrWhiteSpace(contentType))
					contentType = System.Web.MimeMapping.GetMimeMapping(headers.ContentDisposition.FileName);

				string fileName = null;

				//设置文件名的初始值
				if(_filePath[_filePath.Length - 1] != '/')
				{
					var index = _filePath.LastIndexOf('/');

					if(index > 0 && index < _filePath.Length - 1)
						fileName = _filePath.Substring(index + 1);
				}

				var dispositionName = UnquoteToken(headers.ContentDisposition.Name);
				var extensionName = System.IO.Path.GetExtension(UnquoteToken(headers.ContentDisposition.FileName));

				if(!string.IsNullOrWhiteSpace(dispositionName))
				{
					//获取请求头中显式指定的文件名（注意：该文件名支持模板格式）
					if(_headers.TryGetValue(dispositionName + ".name", out var value))
						fileName = Zongsoft.Text.TemplateEvaluatorManager.Default.Evaluate<string>(value, null).ToLowerInvariant() + extensionName;
				}

				//定义文件写入的模式
				bool overwrite = false;

				//定义文件的扩展名是否自动添加
				bool extensionAppend = true;

				//执行写入前的回调方法
				if(_onWriting != null)
				{
					//创建回调参数
					var args = new WritingEventArgs(_filePath, fileName, Interlocked.Increment(ref _index));

					//执行写入前的回调
					_onWriting(args);

					if(args.Cancel)
						return null;

					//获取文件写入参数
					fileName = args.FileName;
					overwrite = args.Overwrite;
					extensionAppend = args.ExtensionAppend;
				}

				//如果文件名为空，则生成一个以“时间戳-随机数.ext”的默认文件名
				if(string.IsNullOrWhiteSpace(fileName))
					fileName = string.Format("X{0}-{1}{2}", ((long)(DateTime.UtcNow - EPOCH).TotalSeconds).ToString(), Zongsoft.Common.RandomGenerator.GenerateString(), extensionAppend ? extensionName : string.Empty);
				else if(extensionAppend && !fileName.EndsWith(extensionName))
					fileName += extensionName;

				//生成文件的完整路径
				var filePath = Zongsoft.IO.Path.Combine(_directory, fileName);

				//生成文件信息的描述实体
				var fileInfo = new Zongsoft.IO.FileInfo(filePath, (headers.ContentDisposition.Size.HasValue ? headers.ContentDisposition.Size.Value : -1), DateTime.Now, null, FileSystem.GetUrl(filePath))
				{
					Type = contentType,
				};

				//将上传的文件项的键名加入到文件描述实体的扩展属性中
				if(!string.IsNullOrWhiteSpace(dispositionName))
					fileInfo.Properties.Add(EXTENDED_PROPERTY_DISPOSITIONNAME, dispositionName);

				//将上传的原始文件名加入到文件描述实体的扩展属性中
				fileInfo.Properties.Add(EXTENDED_PROPERTY_FILENAME, Uri.UnescapeDataString(UnquoteToken(headers.ContentDisposition.FileName)));

				if(_headers != null && _headers.Count > 0 && !string.IsNullOrWhiteSpace(dispositionName))
				{
					//从全局头里面查找当前上传文件的自定义属性
					foreach(var header in _headers)
					{
						if(header.Key.Length > dispositionName.Length + 1 && header.Key.StartsWith(dispositionName + "-", StringComparison.OrdinalIgnoreCase))
							fileInfo.Properties.Add(header.Key.Substring(dispositionName.Length + 1), header.Value);
					}
				}

				//将文件信息对象加入到集合中
				_fileData.Add(fileInfo);

				//在表单数据标记列表中按顺序将当前内容标记为非普通表单域（即二进制文件域）
				_isFormData.Add(false);

				try
				{
					//调用文件系统根据完整文件路径去创建一个新建文件流
					var stream = FileSystem.File.Open(filePath, (overwrite ? FileMode.Create : FileMode.CreateNew), FileAccess.Write, (fileInfo.HasProperties ? fileInfo.Properties : null));

					fileInfo.Properties.Add(EXTENDED_PROPERTY_FILESTREAM, stream);

					return stream;
				}
				catch
				{
					if(fileInfo != null)
						_fileData.Remove(fileInfo);

					throw;
				}
			}

			public override async Task ExecutePostProcessingAsync()
			{
				int index = 0;
				int fileIndex = 0;

				foreach(var content in this.Contents)
				{
					if(_isFormData[index++])
					{
						_formData.Add(UnquoteToken(content.Headers.ContentDisposition.Name), await content.ReadAsStringAsync());
					}
					else
					{
						Stream stream;
						var fileInfo = _fileData[fileIndex++];

						if(fileInfo != null && fileInfo.Properties.TryGetValue(EXTENDED_PROPERTY_FILESTREAM, out stream) && stream != null)
						{
							fileInfo.Size = this.GetStramLength(stream);
							fileInfo.Properties.Remove(EXTENDED_PROPERTY_FILESTREAM);
						}
					}
				}
			}
			#endregion

			#region 私有方法
			private string UnquoteToken(string token)
			{
				if(string.IsNullOrWhiteSpace(token))
					return string.Empty;

				if(token.Length > 1 && token[0] == '"' && token[token.Length - 1] == '"')
					return token.Substring(1, token.Length - 2);

				return token;
			}

			private long GetStramLength(Stream stream)
			{
				if(stream == null)
					return 0;

				try
				{
					return stream.Length;
				}
				catch
				{
					return -1;
				}
			}
			#endregion
		}
		#endregion
	}
}

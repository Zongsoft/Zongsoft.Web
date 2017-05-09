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
using System.Threading.Tasks;

namespace Zongsoft.Web
{
	internal static class TaskHelper
	{
		private struct AsyncVoid
		{
		}

		private static readonly Task _defaultCompleted = Task.FromResult<AsyncVoid>(default(AsyncVoid));
		private static readonly Task<object> _completedTaskReturningNull = Task.FromResult<object>(null);

		/// <summary>
		/// 返回一个被取消的任务。
		/// </summary>
		internal static Task Canceled()
		{
			return CancelCache<AsyncVoid>.Canceled;
		}

		/// <summary>
		/// 返回一个被取消的特定类型的任务。
		/// </summary>
		internal static Task<TResult> Canceled<TResult>()
		{
			return CancelCache<TResult>.Canceled;
		}

		/// <summary>
		/// 返回一个没有结果的已完成的任务。
		/// </summary>        
		internal static Task Completed()
		{
			return _defaultCompleted;
		}

		/// <summary>
		/// 返回一个出错的任务。
		/// </summary>
		internal static Task FromError(Exception exception)
		{
			return FromError<AsyncVoid>(exception);
		}

		/// <summary>
		/// 返回一个出错的特定类型的任务
		/// </summary>
		internal static Task<TResult> FromError<TResult>(Exception exception)
		{
			TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
			tcs.SetException(exception);
			return tcs.Task;
		}

		internal static Task<object> NullResult()
		{
			return _completedTaskReturningNull;
		}

		private static class CancelCache<TResult>
		{
			public static readonly Task<TResult> Canceled = GetCancelledTask();

			private static Task<TResult> GetCancelledTask()
			{
				TaskCompletionSource<TResult> tcs = new TaskCompletionSource<TResult>();
				tcs.SetCanceled();
				return tcs.Task;
			}
		}
	}
}

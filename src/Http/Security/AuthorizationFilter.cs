using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Zongsoft.Web.Http.Security
{
	public class AuthorizationFilter : System.Web.Http.Filters.IAuthorizationFilter
	{
		public bool AllowMultiple
		{
			get
			{
				return true;
			}
		}

		public Task<HttpResponseMessage> ExecuteAuthorizationFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
		{
			return continuation();
		}
	}
}

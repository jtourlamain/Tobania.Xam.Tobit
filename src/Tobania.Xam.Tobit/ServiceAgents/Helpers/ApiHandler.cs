using System;
using System.Threading.Tasks;
using System.Net.Http;
using ModernHttpClient;
using System.Net.Http.Headers;

namespace Tobania.Xam.Tobit.ServiceAgents.Helpers
{
	public class ApiHandler: NativeMessageHandler
	{
	private readonly Func<string> getToken;


		public ApiHandler(Func<string> getToken)
		{
			this.getToken = getToken;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
		{
			var auth = request.Headers.Authorization;
			if (auth != null)
			{
				var token = getToken();
				request.Headers.Authorization = new AuthenticationHeaderValue(auth.Scheme, token);
			}
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}


namespace UserSerivce.Utilities
{
	/// <summary>
	/// 
	/// </summary>
	[Microsoft.AspNetCore.Mvc.ApiController]

	[Microsoft.AspNetCore.Mvc.Route
		(template: Constants.Routing.Controller)]
	public class ControllerBaseTemp : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="databaseContext"></param>
		public ControllerBaseTemp(Persistence.DataBaseContext databaseContext)
		{
			DatabaseContext = databaseContext;
		}

		/// <summary>
		/// 
		/// </summary>
		protected Persistence.DataBaseContext DatabaseContext { get; }
	}
}

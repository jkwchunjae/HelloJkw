using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class AccessDeniedException: Exception
	{
		public AccessDeniedException() { }
	}

	public class InValidAccountIdException: Exception
	{
		public InValidAccountIdException() { }
	}

	public class RegistrationFailException: Exception
	{
		public RegistrationFailException() { }
	}
}

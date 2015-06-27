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
	public class NotRegisteredUserException: Exception
	{
		public NotRegisteredUserException() { }
	}

	public class InValidAccountIdException: Exception
	{
		public InValidAccountIdException() { }
	}

	public class AlreadyRegisterdException: Exception
	{
		public AlreadyRegisterdException() { }
	}

	public class RegistrationFailException: Exception
	{
		public RegistrationFailException() { }
	}
}

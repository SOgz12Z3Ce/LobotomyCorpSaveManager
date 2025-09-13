using System;

namespace LobotomyCorpSaveManager.Exceptions
{
	public class UnreachableCodeException : Exception
	{
		public UnreachableCodeException()
		{
		}

		public UnreachableCodeException(string message) : base(message)
		{
		}

		public UnreachableCodeException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

using System;

namespace LobotomyCorpSaveManager.Exceptions
{
	public class BadFileException : Exception
	{
		public BadFileException()
		{
		}

		public BadFileException(string message) : base(message)
		{
		}

		public BadFileException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

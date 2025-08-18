using System;

namespace LobotomyCorpSaveManager.Exceptions
{
	public class BadEntryNodeIdException : Exception
	{
		public BadEntryNodeIdException()
		{
		}

		public BadEntryNodeIdException(string message) : base(message)
		{
		}

		public BadEntryNodeIdException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}

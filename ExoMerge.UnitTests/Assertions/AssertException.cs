using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.UnitTests.Assertions
{
	#region AssertExceptionBase<TException>

	public class AssertExceptionBase<TException>
		where TException : Exception
	{
		internal AssertExceptionBase(string expectedMessage)
		{
			ExpectedMessage = expectedMessage;
		}

		protected string ExpectedMessage { get; private set; }

		protected void PerformActionAndAssert(Action action)
		{
			bool capturedError = false;

			try
			{
				action();
			}
			catch (Exception e)
			{
				Assert.IsTrue(e is TException, string.Format("Expected exception of type {0} but found type {1}", typeof(TException).Name, e.GetType().Name));

				if (ExpectedMessage != null)
					Assert.AreEqual(ExpectedMessage, e.Message, string.Format("Expected exception message to be \"{0}\" but it was \"{1}\"", ExpectedMessage, e.Message));

				capturedError = true;
			}

			Assert.IsTrue(capturedError, string.Format("Expected exception of type {0}", typeof(TException).Name));
		}

		public void IsThrownBy(Action action)
		{
			PerformActionAndAssert(action);
		}
	}

	#endregion

	#region AssertTypedException<TException>

	public class AssertTypedException<TException> : AssertExceptionBase<TException>
		where TException : Exception
	{
		internal AssertTypedException()
			: base(null)
		{
		}

		public AssertTypedExceptionWithMessage<TException> WithMessage(string expectedMessage)
		{
			return new AssertTypedExceptionWithMessage<TException>(expectedMessage);
		}
	}

	#endregion

	#region AssertTypedExceptionWithMessage<TException>

	public class AssertTypedExceptionWithMessage<TException> : AssertExceptionBase<TException>
		where TException : Exception
	{
		internal AssertTypedExceptionWithMessage(string expectedMessage)
			: base(expectedMessage)
		{
		}
	}

	#endregion

	#region AssertExceptionWithMessage

	public class AssertExceptionWithMessage : AssertExceptionBase<Exception>
	{
		internal AssertExceptionWithMessage(string expectedMessage)
			: base(expectedMessage)
		{
		}

		public AssertTypedExceptionWithMessage<TException> OfType<TException>()
			where TException : Exception
		{
			return new AssertTypedExceptionWithMessage<TException>(ExpectedMessage);
		}
	}

	#endregion

	#region AssertException

	public static class AssertException
	{
		public static AssertTypedException<TException> OfType<TException>()
			where TException : Exception
		{
			return new AssertTypedException<TException>();
		}

		public static AssertExceptionWithMessage WithMessage(string expectedMessage)
		{
			return new AssertExceptionWithMessage(expectedMessage);
		}

		public static void IsThrownBy(Action action)
		{
			new AssertTypedException<Exception>().IsThrownBy(action);
		}
	}

	#endregion
}

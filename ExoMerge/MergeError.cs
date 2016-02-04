using System;
using ExoMerge.Analysis;

namespace ExoMerge
{
	/// <summary>
	/// Represents an error in the merge process. Sub-classes may provide more
	/// contextual information, such as the location of the error in the
	/// template, or suggestions for how to resolve the error.
	/// </summary>
	public interface IMergeError
	{
		/// <summary>
		/// The type of error.
		/// </summary>
		MergeErrorType Type { get; }

		/// <summary>
		/// The token where the error occurred.
		/// </summary>
		IToken Token { get; }

		/// <summary>
		/// The exception that was thrown, if any.
		/// </summary>
		Exception Exception { get; }
	}

	/// <summary>
	/// Represents an error in the merge process. Sub-classes may provide more
	/// contextual information, such as the location of the error in the
	/// template, or suggestions for how to resolve the error.
	/// </summary>
	public abstract class MergeError<TToken> : IMergeError
		where TToken : class, IToken
	{
		internal MergeError(MergeErrorType type, TToken token, Exception exception = null)
		{
			Type = type;
			Token = token;
			Exception = exception;
		}

		/// <summary>
		/// The type of error.
		/// </summary>
		public MergeErrorType Type { get; private set; }

		/// <summary>
		/// The token where the error occurred.
		/// </summary>
		public TToken Token { get; private set; }

		/// <summary>
		/// The token where the error occurred.
		/// </summary>
		IToken IMergeError.Token
		{
			get { return Token; }
		}

		/// <summary>
		/// The exception that was thrown, if any.
		/// </summary>
		public Exception Exception { get; private set; }
	}

	/// <summary>
	/// Represents the type of error that occurred during merge.
	/// </summary>
	public enum MergeErrorType
	{
		/// <summary>
		/// The type of error is not known or was not specified.
		/// </summary>
		Unspecified,

		/// <summary>
		/// An error occur during compilation of the template.
		/// </summary>
		Compilation,

		/// <summary>
		/// An error occured while accessing data during merge.
		/// </summary>
		DataAccess,

		/// <summary>
		/// An error occurred while manipulating the merge target.
		/// </summary>
		Manipulation,
	}

	/// <summary>
	/// An exception that occurred during merging.
	/// </summary>
	public class MergeException : Exception
	{
		internal MergeException(IMergeError[] errors)
			: base(GenerateMessage(errors))
		{
			Errors = errors;
		}

		internal MergeException(string message, IMergeError[] errors)
			: base(message)
		{
			Errors = errors;
		}

		/// <summary>
		/// Gets the errors that were encountered that lead to the exception.
		/// </summary>
		public IMergeError[] Errors { get; private set; }

		/// <summary>
		/// Generate an exception message for the given errors.
		/// </summary>
		private static string GenerateMessage(IMergeError[] errors)
		{
			// If for some reason a caller doesn't provide errors, then use a generic message.
			if (errors.Length == 0)
				return "Error(s) occurred during merge";

			if (errors.Length == 1)
				return "An error occurred during merge" + (errors[0].Exception != null ? ": " + errors[0].Exception.Message : "");

			return string.Format("A total of {0} errors occurred during merge", errors.Length);
		}
	}

	/// <summary>
	/// Options for how to deal with errors during merge.
	/// </summary>
	public enum MergeErrorAction
	{
		/// <summary>
		/// Stop immediately if an error is encountered.
		/// </summary>
		Stop,

		/// <summary>
		/// Continue when an error is encountered, but don't ignore it.
		/// </summary>
		Continue,

		/// <summary>
		/// Attempt to ignore any errors that are encountered and continue merging.
		/// </summary>
		Ignore
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ExoMerge.Documents;

namespace ExoMerge.UnitTests.Common
{
	public static class LoggingDocumentAdapter
	{
		public static LoggingDocumentAdapter<TDocument, TNode> Create<TDocument, TNode>(IDocumentAdapter<TDocument, TNode> adapter)
		{
			return new LoggingDocumentAdapter<TDocument, TNode>(adapter);
		}
	}

	public class LoggingDocumentAdapter<TDocument, TNode> : IDocumentAdapter<TDocument, TNode>, IDisposable
	{
		private readonly IDocumentAdapter<TDocument, TNode> adapter;

		private readonly StringBuilder builder = new StringBuilder();

		public LoggingDocumentAdapter(IDocumentAdapter<TDocument, TNode> adapter)
		{
			this.adapter = adapter;
		}

		public string Path { get; set; }

		public Action<string> Before { get; set; }

		public Action<string> After { get; set; }

		private void Log(string format, params object[] args)
		{
			builder.AppendLine(args.Length == 0 ? format : string.Format(format, args));
		}

		private void FlushLog()
		{
			if (!string.IsNullOrEmpty(Path))
				File.WriteAllText(Path, builder.ToString());

			builder.Length = 0;
		}

		public void Dispose()
		{
			if (!string.IsNullOrEmpty(Path))
				FlushLog();
		}

		public DocumentNodeType GetNodeType(TNode node)
		{
			Log("GetNodeType");
			if (Before != null)
				Before("GetNodeType");
			var result = adapter.GetNodeType(node);
			if (After != null)
				After("GetNodeType");
			return result;
		}

		public TNode GetParent(TNode node)
		{
			Log("GetParent");
			if (Before != null)
				Before("GetParent");
			var result = adapter.GetParent(node);
			if (After != null)
				After("GetParent");
			return result;
		}

		public TNode GetAncestor(TNode node, DocumentNodeType type)
		{
			Log("GetAncestor");
			if (Before != null)
				Before("GetAncestor");
			var result = adapter.GetAncestor(node, type);
			if (After != null)
				After("GetAncestor");
			return result;
		}

		public TNode GetNextSibling(TNode node)
		{
			Log("GetNextSibling");
			if (Before != null)
				Before("GetNextSibling");
			var result = adapter.GetNextSibling(node);
			if (After != null)
				After("GetNextSibling");
			return result;
		}

		public TNode GetPreviousSibling(TNode node)
		{
			Log("GetPreviousSibling");
			if (Before != null)
				Before("GetPreviousSibling");
			var result = adapter.GetPreviousSibling(node);
			if (After != null)
				After("GetPreviousSibling");
			return result;
		}

		public void Remove(TNode node)
		{
			Log("Remove");
			if (Before != null)
				Before("Remove");
			adapter.Remove(node);
			if (After != null)
				After("Remove");
		}

		public IEnumerable<TNode> GetChildren(TDocument document)
		{
			Log("GetChildren");
			if (Before != null)
				Before("GetChildren");
			var result = adapter.GetChildren(document).ToArray();
			if (After != null)
				After("GetChildren");
			return result;
		}

		public bool IsComposite(TNode node)
		{
			Log("IsComposite");
			if (Before != null)
				Before("IsComposite");
			var result = adapter.IsComposite(node);
			if (After != null)
				After("IsComposite");
			return result;
		}

		public IEnumerable<TNode> GetChildren(TNode parent)
		{
			Log("GetChildren");
			if (Before != null)
				Before("GetChildren");
			var result = adapter.GetChildren(parent);
			if (After != null)
				After("GetChildren");
			return result;
		}

		public TNode GetFirstChild(TNode parent)
		{
			Log("GetFirstChild");
			if (Before != null)
				Before("GetFirstChild");
			var result = adapter.GetFirstChild(parent);
			if (After != null)
				After("GetFirstChild");
			return result;
		}

		public TNode GetLastChild(TNode parent)
		{
			Log("GetLastChild");
			if (Before != null)
				Before("GetLastChild");
			var result = adapter.GetLastChild(parent);
			if (After != null)
				After("GetLastChild");
			return result;
		}

		public TNode CreateTextRun(TDocument document, string text)
		{
			Log("CreateTextRun");
			if (Before != null)
				Before("CreateTextRun");
			var result = adapter.CreateTextRun(document, text);
			if (After != null)
				After("CreateTextRun");
			return result;
		}

		public string GetText(TNode node)
		{
			Log("GetText");
			if (Before != null)
				Before("GetText");
			var result = adapter.GetText(node);
			if (After != null)
				After("GetText");
			return result;
		}

		public void SetText(TNode node, string text)
		{
			Log("SetText");
			if (Before != null)
				Before("SetText");
			adapter.SetText(node, text);
			if (After != null)
				After("SetText");
		}

		public bool IsNonVisibleMarker(TNode node)
		{
			Log("IsNonVisibleMarker");
			if (Before != null)
				Before("IsNonVisibleMarker");
			var result = adapter.IsNonVisibleMarker(node);
			if (After != null)
				After("IsNonVisibleMarker");
			return result;
		}

		public void InsertAfter(TNode newNode, TNode insertAfter)
		{
			Log("InsertAfter");
			if (Before != null)
				Before("InsertAfter");
			adapter.InsertAfter(newNode, insertAfter);
			if (After != null)
				After("InsertAfter");
		}

		public void InsertBefore(TNode newNode, TNode insertBefore)
		{
			Log("InsertBefore");
			if (Before != null)
				Before("InsertBefore");
			adapter.InsertBefore(newNode, insertBefore);
			if (After != null)
				After("InsertBefore");
		}

		public void AppendChild(TNode parent, TNode newChild)
		{
			Log("AppendChild");
			if (Before != null)
				Before("AppendChild");
			adapter.AppendChild(parent, newChild);
			if (After != null)
				After("AppendChild");
		}

		public TNode Clone(TNode node, bool recursive)
		{
			Log("Clone");
			if (Before != null)
				Before("Clone");
			var result = adapter.Clone(node, recursive);
			if (After != null)
				After("Clone");
			return result;
		}
	}
}

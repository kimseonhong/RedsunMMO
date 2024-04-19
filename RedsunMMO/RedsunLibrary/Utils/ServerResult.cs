using System;

namespace RedsunLibrary.Utils
{
	public enum EResultType
	{
		_NONE = 0,

		OK = 1,
		FAIL = 2,
		EXCEPTION = 3,

		_END
	}

	public class ServerResult
	{
		private EResultType _resultType;
		private Int32 _resultCode;
		private string _descritpion;

		public static ServerResult alloc() => new ServerResult();

		public Int32 ResultCode => _resultCode;

		public ServerResult()
		{
			_resultType = EResultType.OK;
			_resultCode = 0;
			_descritpion = string.Empty;
		}

		public ServerResult setOk()
		{
			_resultType = EResultType.OK;
			_resultCode = 0;
			return this;
		}

		public ServerResult setOk(int resultCode)
		{
			_resultType = EResultType.OK;
			_resultCode = resultCode;
			return this;
		}

		public ServerResult setOk(string description)
		{
			_resultType = EResultType.OK;
			_resultCode = 0;
			_descritpion = description;
			return this;
		}

		public ServerResult setOk(int resultCode, string description)
		{
			_resultType = EResultType.OK;
			_resultCode = resultCode;
			_descritpion = description;
			return this;
		}

		public ServerResult setFail()
		{
			_resultType = EResultType.FAIL;
			_resultCode = 0;
			return this;
		}

		public ServerResult setFail(int resultCode)
		{
			_resultType = EResultType.FAIL;
			_resultCode = resultCode;
			return this;
		}

		public ServerResult setFail(string description)
		{
			_resultType = EResultType.FAIL;
			_resultCode = 0;
			_descritpion = description;
			return this;
		}

		public ServerResult setFail(int resultCode, string description)
		{
			_resultType = EResultType.FAIL;
			_resultCode = resultCode;
			_descritpion = description;
			return this;
		}

		public ServerResult setException()
		{
			_resultType = EResultType.FAIL;
			_resultCode = 0;
			return this;
		}

		public ServerResult setException(int resultCode)
		{
			_resultType = EResultType.FAIL;
			_resultCode = resultCode;
			return this;
		}

		public ServerResult setException(string description)
		{
			_resultType = EResultType.FAIL;
			_resultCode = 0;
			_descritpion = description;
			return this;
		}

		public ServerResult setException(int resultCode, string description)
		{
			_resultType = EResultType.FAIL;
			_resultCode = resultCode;
			_descritpion = description;
			return this;
		}

		public bool IsOk() => _resultType == EResultType.OK;

		public bool IsFail() => _resultType == EResultType.FAIL || _resultType == EResultType.EXCEPTION;

		public bool IsException() => _resultType == EResultType.EXCEPTION;

		public override string ToString() => $"[{_resultType}({_resultCode})]: {_descritpion}";
	}
}

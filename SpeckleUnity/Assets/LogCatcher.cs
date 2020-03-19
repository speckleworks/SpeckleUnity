using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogCatcher : MonoBehaviour
{
	public static LogFile logFile = new LogFile ();

	public void OnEnable ()
	{
		Application.logMessageReceived += HandleLog;
	}

	public void OnDisable ()
	{
		Application.logMessageReceived -= HandleLog;
	}

	public void HandleLog (string logString, string stackTrace, LogType type)
	{
		logFile.logs.Add (new Log (logString, stackTrace, type));

		File.WriteAllText (Application.persistentDataPath + "/logs.json", JsonUtility.ToJson (logFile));
	}

	public class LogFile
	{
		public List<Log> logs = new List<Log> ();

		public LogFile ()
		{
			logs = new List<Log> ();
		}
	}

	[Serializable]
	public class Log
	{
		public string logType;
		public string message;
		public string stackTrace;

		public Log (string logString, string stackTrace, LogType type)
		{
			message = logString;
			this.stackTrace = stackTrace;

			switch (type)
			{
				case LogType.Log:
					logType = "Log";
					break;
				case LogType.Warning:
					logType = "Warning";
					break;
				case LogType.Error:
					logType = "Error";
					break;
				case LogType.Exception:
					logType = "Exception";
					break;
			}
		}
	}
}

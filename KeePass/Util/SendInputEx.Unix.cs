﻿/*
  KeePass Password Safe - The Open-Source Password Manager
  Copyright (C) 2003-2011 Dominik Reichl <dominik.reichl@t-online.de>

  This program is free software; you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation; either version 2 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program; if not, write to the Free Software
  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using KeePass.Native;

using KeePassLib.Utility;

namespace KeePass.Util
{
	public static partial class SendInputEx
	{
		private static Dictionary<string, string> m_dictXDoReplace = null;
		private static void OSSendKeysUnix(string strSequence)
		{
			if(m_dictXDoReplace == null) // From keysymdef.h:
			{
				m_dictXDoReplace = new Dictionary<string, string>();

				m_dictXDoReplace["tab"] = "Tab";
				m_dictXDoReplace["enter"] = "Return";
				m_dictXDoReplace["up"] = "Up";
				m_dictXDoReplace["down"] = "Down";
				m_dictXDoReplace["left"] = "Left";
				m_dictXDoReplace["right"] = "Right";
				m_dictXDoReplace["insert"] = "Insert";
				m_dictXDoReplace["ins"] = "Insert";
				m_dictXDoReplace["delete"] = "Delete";
				m_dictXDoReplace["del"] = "Delete";
				m_dictXDoReplace["home"] = "Home";
				m_dictXDoReplace["end"] = "End";
				m_dictXDoReplace["pgup"] = "Page_Up";
				m_dictXDoReplace["pgdn"] = "Page_Down";
				m_dictXDoReplace["backspace"] = "BackSpace";
				m_dictXDoReplace["bs"] = "BackSpace";
				m_dictXDoReplace["bksp"] = "BackSpace";
				m_dictXDoReplace["break"] = "Break";
				m_dictXDoReplace["capslock"] = "Caps_Lock";
				m_dictXDoReplace["esc"] = "Escape";
				m_dictXDoReplace["help"] = "Help";
				m_dictXDoReplace["numlock"] = "Num_Lock";
				m_dictXDoReplace["prtsc"] = "Print";
				m_dictXDoReplace["scrolllock"] = "Scroll_Lock";

				for(int f = 1; f <= 16; ++f)
					m_dictXDoReplace["f" + f.ToString()] = ("F" + f.ToString());

				m_dictXDoReplace["add"] = "KP_Add";
				m_dictXDoReplace["subtract"] = "KP_Subtract";
				m_dictXDoReplace["multiply"] = "KP_Multiply";
				m_dictXDoReplace["divide"] = "KP_Divide";

				m_dictXDoReplace[@"+"] = "plus";
				m_dictXDoReplace[@"^"] = "asciicircum";
				m_dictXDoReplace[@"%"] = "percent";
				m_dictXDoReplace[@"~"] = "asciitilde";
				m_dictXDoReplace[@"("] = "parenleft";
				m_dictXDoReplace[@")"] = "parenright";
				m_dictXDoReplace[@"{"] = "braceleft";
				m_dictXDoReplace[@"}"] = "braceright";

				foreach(string strKpKey in m_dictXDoReplace.Keys)
				{
					Debug.Assert(strKpKey.ToLower() == strKpKey);
				}
			}

			// Support ~ as {ENTER}
			const string strTildeEnc = @"70BF02D7-B169-4A9C-9E5A-CEB821279EC4";
			strSequence = strSequence.Replace(@"{~}", strTildeEnc);
			strSequence = strSequence.Replace(@"~", @"{ENTER}");
			strSequence = strSequence.Replace(strTildeEnc, @"{~}");

			StringBuilder sb = new StringBuilder();
			bool bInCode = false;
			for(int i = 0; i < strSequence.Length; ++i)
			{
				char ch = strSequence[i];

				if((ch == '{') && !bInCode)
				{
					if(sb.Length > 0)
					{
						OSSendKeysUnixString(sb.ToString());
						sb.Remove(0, sb.Length);
					}

					bInCode = true;
				}
				else if((ch == '}') && bInCode)
				{
					if(sb.Length > 0)
					{
						string strKeyCode = sb.ToString();
						string strKeyLow = strKeyCode.ToLower();

						string strRep;
						if(m_dictXDoReplace.TryGetValue(strKeyLow, out strRep))
							strKeyCode = strRep;

						if(!strKeyCode.StartsWith("Delay", StrUtil.CaseIgnoreCmp))
							OSSendKeysUnixKey(strKeyCode);

						sb.Remove(0, sb.Length);
					}

					bInCode = false;
				}
				else sb.Append(ch);
			}

			if(sb.Length > 0) OSSendKeysUnixString(sb.ToString());
		}

		private static void OSSendKeysUnixKey(string strKey)
		{
			NativeMethods.RunXDoTool("key --clearmodifiers " + strKey);
		}

		private static void OSSendKeysUnixString(string strString)
		{
			NativeMethods.RunXDoTool(@"type --clearmodifiers '" + strString + @"'");
		}
	}
}

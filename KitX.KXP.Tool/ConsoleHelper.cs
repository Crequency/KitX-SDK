// SPDX-FileCopyrightText: 2022 Crequency Studio
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Text;

namespace KitX.KXP.Tool
{
    internal static class ConsoleHelper
    {
        internal static void WriteSeparater(char sep = '-')
        {
            Console.WriteLine(new StringBuilder().Append(sep, Console.WindowWidth).ToString());
        }

        internal static void DoInAnotherColor(ConsoleColor cc = ConsoleColor.White, Action? action = null)
        {
            ConsoleColor nowColor = Console.ForegroundColor;
            Console.ForegroundColor = cc;
            action?.Invoke();
            Console.ForegroundColor = nowColor;
        }
    }
}

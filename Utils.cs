﻿namespace Cephalon {
    using System;

    public static class Utils {

        public static string RandomPick(string listText) {
            var list = listText.Split('|');
            var random = new Random();
            return list[random.Next(list.Length)];
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchTextToVoice.TwitchIntegration
{
    public class TokenResponseEntity
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public string? error { get; set; }
    }
}

﻿using Microsoft.OpenApi.Models;
using NLog.Config;
using NLog.Targets;
using NLog;

using WebStudyServer.GAME;
using NLog.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebStudyServer
{
    public partial class Startup
    {
        public void Logging(WebApplicationBuilder builder)
        {
            var logLevel = APP.Cfg.LogLevel;

            builder.Logging.ClearProviders(); // 기본 로깅 제공자 제거
            builder.Logging.AddNLog(); // NLog 사용 설정
            builder.Logging.SetMinimumLevel(logLevel); // 로깅 최소 수준 설정

            var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "..", "logs");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory); // 디렉터리가 없으면 생성
            }

            // NLog 설정 로드
            var logConfigDirectory = Path.Combine(Directory.GetCurrentDirectory(), "NLog.config");
            LogManager.Setup().LoadConfigurationFromFile(logConfigDirectory);
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Init Log");
        }
    }
}

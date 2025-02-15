﻿using SecureLens.Application.Analysis.Results;
using SecureLens.Core.Models;

namespace SecureLens.Application.Analysis.Interfaces;

public interface ITerminalStatisticsCalculator
{
    public List<TerminalStatisticsRow> ComputeTerminalStatistics(List<CompletedUser> completedUsers,
        List<AdminByRequestSetting> settings);
}
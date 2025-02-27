/*
BSD 3-Clause License

Copyright (c) 2024, Jooty

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.
*/

using System;
using Imcodec.ObjectProperty;
using Imcodec.ObjectProperty.TypeCache;
using Imview.PacketReader.Services;

namespace Imview.PacketReader;

public sealed class QuestBuilder {

    public static async Task<List<QuestTemplate>> BuildQuestsFromPacketCaptureAsync(string packetCapturePath) {
        var questOfferPackets = await PacketReaderService.ExtractPacketsAsync<QuestOfferPacket>(
            packetCapturePath,
            "MSG_QUESTOFFER"
        );

        return [.. questOfferPackets.Select(CovertOfferPacketToTemplate)];
    }

    private static QuestTemplate CovertOfferPacketToTemplate(QuestOfferPacket packet) {
        // Craft the initial template.
        var template = new QuestTemplate {
            m_questName = packet.QuestName,
            m_questTitle = packet.QuestTitle,
            m_questLevel = packet.Level,
            m_mainline = packet.Mainline == 1,
        };

        // Extract the goal compilation from the packet.
        var goalCompilation = ExtractGoalCompilationFromPacket(packet);

        if (goalCompilation is not null) {
            template.m_goals = CraftGoalsFromCompilation(goalCompilation!);
        }

        return template;
    }

    private static GoalCompilation? ExtractGoalCompilationFromPacket(QuestOfferPacket packet) {
        try {
            var goalBlob = packet.GoalData.Replace(" ", string.Empty);
            var goalBlobBytes = Convert.FromHexString(goalBlob);
            var serializer = new ObjectSerializer(false, SerializerFlags.None);
            if (!serializer.Deserialize<GoalCompilation>(goalBlobBytes, 1, out var goalCompilation)) {
                return null;
            }

            return goalCompilation;
        }
        catch {
            return null;
        }
    }

    private static List<GoalTemplate> CraftGoalsFromCompilation(GoalCompilation goalCompilation) {
        var goals = new List<GoalTemplate>();

        foreach (var goal in goalCompilation.m_goals) {
            var goalTemplate = new GoalTemplate {
                m_goalNameID = goal.m_goalNameID,
                m_goalTitle = goal.m_goalTitle,
                m_locationName = goal.m_goalLocation,
                m_destinationZone = goal.m_goalDestinationZone,
                m_displayImage1 = goal.m_goalImage1,
                m_displayImage2 = goal.m_goalImage2,
                m_goalType = (GOAL_TYPE) goal.m_goalType,
            };

            goals.Add(goalTemplate);
        }

        return goals;
    }

}

public class QuestOfferPacket {

    [PacketReaderService.PacketField("MobileID", PacketReaderService.ExtractMethod.Gid)]
    public ulong MobileID { get; set; }

    [PacketReaderService.PacketField("QuestName", PacketReaderService.ExtractMethod.ASCII)]
    public string QuestName { get; set; } = string.Empty;

    [PacketReaderService.PacketField("QuestTitle", PacketReaderService.ExtractMethod.ASCII)]
    public string QuestTitle { get; set; } = string.Empty;

    [PacketReaderService.PacketField("QuestInfo", PacketReaderService.ExtractMethod.ASCII)]
    public string QuestInfo { get; set; } = string.Empty;

    [PacketReaderService.PacketField("Level")]
    public int Level { get; set; }

    [PacketReaderService.PacketField("Rewards", PacketReaderService.ExtractMethod.Hex)]
    public string Rewards { get; set; } = string.Empty;

    [PacketReaderService.PacketField("GoalData", PacketReaderService.ExtractMethod.Hex)]
    public string GoalData { get; set; } = string.Empty;

    [PacketReaderService.PacketField("Mainline", PacketReaderService.ExtractMethod.Ubyte)]
    public byte Mainline { get; set; }

}
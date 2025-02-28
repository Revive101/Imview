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

    private static readonly Dictionary<QuestTemplate, ulong> s_questTemplateIDMap = [];

    public static async Task<List<QuestTemplate>> BuildQuestsFromPacketCaptureAsync(string packetCapturePath) {
        var questOfferPackets = await PacketReaderService.ExtractPacketsAsync<QuestOfferPacket>(
            packetCapturePath,
            "MSG_QUESTOFFER"
        );
        var sendQuestPackets = await PacketReaderService.ExtractPacketsAsync<SendQuestPacket>(
            packetCapturePath,
            "MSG_SENDQUEST"
        );
        var sendGoalPackets = await PacketReaderService.ExtractPacketsAsync<SendGoalPacket>(
            packetCapturePath,
            "MSG_SENDGOAL"
        );

        var templates = questOfferPackets
            .Select(CovertOfferPacketToTemplate)
            .ToList();
        foreach (var template in templates) {
            AddQuestIDToQuestTemplate(template, sendQuestPackets);
            AddGoalsToQuestTemplate(template, sendGoalPackets);
        }

        return templates;
    }

    private static QuestTemplate CovertOfferPacketToTemplate(QuestOfferPacket packet) {
        // Craft the initial template.
        var template = new QuestTemplate {
            m_questName = packet.QuestName,
            m_questTitle = packet.QuestTitle,
            m_questLevel = packet.Level,
            m_mainline = packet.Mainline == 1,
            m_goals = [],
            m_startGoals = [],
        };

        // Extract the goal compilation from the packet.
        var goalCompilation = ExtractGoalCompilationFromPacket(packet);

        if (goalCompilation is not null) {
            template.m_goals = CraftGoalsFromCompilation(goalCompilation!);
        }

        return template;
    }

    private static void AddQuestIDToQuestTemplate(QuestTemplate template, List<SendQuestPacket> packets) {
        // Search through the packets until we find where the quest title matches.
        foreach (var packet in packets) {
            if (packet.QuestTitle == template.m_questTitle) {
                s_questTemplateIDMap[template] = packet.QuestID;

                return;
            }
        }
    }

    private static void AddGoalsToQuestTemplate(QuestTemplate template, List<SendGoalPacket> packets) {
        if (!s_questTemplateIDMap.TryGetValue(template, out var questID)) {
            return;
        }

        // Search through the packets until we find where the quest title matches.
        var counter = 0;
        foreach (var packet in packets) {
            if (packet.QuestID == questID) {
                // If the template already has this goal, skip.
                if (template.m_goals.Any(g => g.m_goalNameID == packet.GoalNameID)) {
                    continue;
                }

                var placeholderGoalName = $"{counter}_{packet.GoalTitle}";
                var goalTemplate = new GoalTemplate {
                    m_goalName = placeholderGoalName,
                    m_goalNameID = packet.GoalNameID,
                    m_goalTitle = packet.GoalTitle,
                    m_locationName = packet.GoalLocation,
                    m_destinationZone = packet.GoalDestinationZone,
                    m_displayImage1 = packet.GoalImage1,
                    m_displayImage2 = packet.GoalImage2,
                    m_goalType = (GOAL_TYPE) packet.GoalType,
                };

                template.m_goals.Add(goalTemplate);
                template.m_startGoals ??= [];
                template.m_startGoals.Add(placeholderGoalName);

                counter++;

                return;
            }
        }
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
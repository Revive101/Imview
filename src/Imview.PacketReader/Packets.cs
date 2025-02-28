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

public class SendQuestPacket {

    [PacketReaderService.PacketField("QuestID", PacketReaderService.ExtractMethod.Gid)]
    public ulong QuestID { get; set;}

    [PacketReaderService.PacketField("QuestTitle", PacketReaderService.ExtractMethod.ASCII)]
    public string? QuestTitle { get; set; }

}

public class SendGoalPacket {

    [PacketReaderService.PacketField("QuestID", PacketReaderService.ExtractMethod.Gid)]
    public ulong QuestID { get; set; }

    [PacketReaderService.PacketField("GoalNameID", PacketReaderService.ExtractMethod.Uint)]
    public uint GoalNameID { get; set; }

    [PacketReaderService.PacketField("GoalTitle", PacketReaderService.ExtractMethod.ASCII)]
    public string? GoalTitle { get; set; }

    [PacketReaderService.PacketField("GoalLocation", PacketReaderService.ExtractMethod.ASCII)]
    public string? GoalLocation { get; set; }

    [PacketReaderService.PacketField("GoalDestinationZone", PacketReaderService.ExtractMethod.ASCII)]
    public string? GoalDestinationZone { get; set; }

    [PacketReaderService.PacketField("GoalImage1", PacketReaderService.ExtractMethod.ASCII)]
    public string? GoalImage1 { get; set; }

    [PacketReaderService.PacketField("GoalImage2", PacketReaderService.ExtractMethod.Hex)]
    public string? GoalImage2 { get; set; }

    [PacketReaderService.PacketField("GoalType", PacketReaderService.ExtractMethod.Ubyte)]
    public byte GoalType { get; set; }

    [PacketReaderService.PacketField("GoalTotal", PacketReaderService.ExtractMethod.Uint)]
    public uint GoalTotal { get; set; }

    [PacketReaderService.PacketField("ClientTags", PacketReaderService.ExtractMethod.Hex)]
    public string? ClientTags { get; set; }

    [PacketReaderService.PacketField("NoQuestHelper", PacketReaderService.ExtractMethod.Ubyte)]
    public byte NoQuestHelper { get; set; }

    [PacketReaderService.PacketField("PetOnlyQuest", PacketReaderService.ExtractMethod.Ubyte)]
    public byte PetOnlyQuest { get; set; }

}
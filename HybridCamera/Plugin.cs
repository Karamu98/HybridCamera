﻿using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DrahsidLib;
using ImGuiNET;
using System;

namespace HybridCamera;

public class Plugin : IDalamudPlugin {
    private IDalamudPluginInterface PluginInterface;
    private IChatGui Chat { get; init; }
    private IClientState ClientState { get; init; }
    private ICommandManager CommandManager { get; init; }

    public string Name => "HybridCamera";

    public Plugin(IDalamudPluginInterface pluginInterface, ICommandManager commandManager, IChatGui chat, IClientState clientState) {
        PluginInterface = pluginInterface;
        Chat = chat;
        ClientState = clientState;
        CommandManager = commandManager;

        DrahsidLib.DrahsidLib.Initialize(pluginInterface, DrawTooltip);

        InitializeCommands();
        InitializeConfig();
        InitializeUI();
        MovementHook.Initialize();
    }

    public static void DrawTooltip(string text) {
        if (ImGui.IsItemHovered() && Globals.Config.HideTooltips == false) {
            ImGui.SetTooltip(text);
        }
    }

    private void InitializeCommands() {
        Commands.Initialize();
    }

    private void InitializeConfig() {
        Globals.Config = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
    }

    private void InitializeUI() {
        Windows.Initialize();
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += Commands.ToggleConfig;
        PluginInterface.UiBuilder.OpenMainUi += Commands.ToggleConfig;
    }

    private unsafe void DrawUI() 
    {
        Windows.System.Draw();
        UpdateCombatState();
        KeybindHook.UpdateKeybindHook();
    }

    private unsafe void UpdateCombatState()
    {
        if(!Globals.Config.functionInCombatOnly)
        {
            return;
        }

        bool playerInCombat = false;
        if (ClientState != null)
        {
            if (ClientState.LocalPlayer != null)
            {
                playerInCombat = ClientState.LocalPlayer.StatusFlags.HasFlag(Dalamud.Game.ClientState.Objects.Enums.StatusFlags.InCombat);
            }
        }

        if(Globals.InCombat && !playerInCombat)
        {
            GameConfig.UiControl.Set("MoveMode", (int)MovementMode.Standard);
        }

        Globals.InCombat = playerInCombat;
    }


    #region IDisposable Support
    protected virtual void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        KeybindHook.Dispose();
        MovementHook.Dispose();

        PluginInterface.SavePluginConfig(Globals.Config);

        PluginInterface.UiBuilder.Draw -= DrawUI;
        Windows.Dispose();
        PluginInterface.UiBuilder.OpenConfigUi -= Commands.ToggleConfig;
        PluginInterface.UiBuilder.OpenMainUi -= Commands.ToggleConfig;

        Commands.Dispose();
    }

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

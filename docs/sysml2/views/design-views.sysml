// Views rendered into docs/design/generated/ for the Design document.
// Reopens each system's package so each 'expose' statement can reference its target part
// def directly. Per SysML2Tools syntax, 'expose' is only valid inside a named 'view' usage
// (not a 'view def' definition), and it - not 'render' - is what scopes the rendered
// diagram content. This repository has two independent system packages (SpdxTool and
// SpdxToolTargets), so SoftwareStructureView is typed by GeneralView - which draws one
// package frame per system - and exposes each system package's members, filtering out
// documentation so the overview stays readable.
package SpdxTool {
    private import StandardViewDefinitions::*;

    view SoftwareStructureView : GeneralView {
        expose SpdxTool::**;
        expose SpdxToolTargets::**;
        filter not @SysML::Documentation;
    }

    view SpdxToolView {
        expose SpdxToolSystem;
    }

    view CommandsView {
        expose CommandsSubsystem;
    }

    view SelfTestView {
        expose SelfTestSubsystem;
    }

    view SpdxView {
        expose SpdxSubsystem;
    }

    view UtilityView {
        expose UtilitySubsystem;
    }
}

package SpdxToolTargets {
    view SpdxToolTargetsView {
        expose SpdxToolTargetsSystem;
    }
}

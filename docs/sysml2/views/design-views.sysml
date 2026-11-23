// Views rendered into docs/design/generated/ for the Design document.
// Per SysML2Tools syntax, 'expose' is only valid inside a named 'view' usage (not a
// 'view def' definition), and it - not 'render' - is what scopes the rendered diagram
// content. This repository has two independent systems (SpdxTool and SpdxToolTargets), so
// SoftwareStructureView is an InterconnectionView that exposes each system's part def
// (one frame per system with its subsystem usages nested inside), with documentation
// filtered out so the overview stays readable.
package SpdxToolDesignViews {
    private import StandardViewDefinitions::*;
    private import SpdxTool::*;
    private import SpdxToolTargets::*;

    view SoftwareStructureView : InterconnectionView {
        expose SpdxTool::SpdxToolSystem;
        expose SpdxToolTargets::SpdxToolTargetsSystem;
        filter not (@SysML::Documentation);
    }

    view SpdxToolView {
        expose SpdxToolSystem;
    }

    view SpdxToolTargetsView {
        expose SpdxToolTargetsSystem;
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

    view OtsView : GeneralView {
        expose OtsDependencies::*;
        filter not (@SysML::Documentation);
    }
}

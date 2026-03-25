{
  description = "Dev environement for Cypherless";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
  };

  outputs =
    {
      self,
      nixpkgs,
      ...
    }:
    let
      system = "x86_64-linux";
      pkgs = import nixpkgs {
        inherit system;
        config.allowUnfree = true;
      };
    in
    {
      devShells.${system}.default = pkgs.mkShell {

        buildInputs = with pkgs; [
          godotPackages_4_6.godot-mono
          jetbrains.rider
        ];

      };
    };
}

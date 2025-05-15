namespace PokemonPocket.Helpers;

public class Selection {

  public string Label;

  public Selection(string label) {
    this.Label = label;
  }

  public override string ToString() => this.Label;

}

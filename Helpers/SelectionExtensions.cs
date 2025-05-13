 namespace PokemonPocket.Helpers;
 using System;

public static class SelectionExtensions {
  public static Selection AsLabel(this string label) {
    return new Selection(label);
  }

  public static Selection WithAction(this string label, Action callback) {
    return new SelectionAction(label, callback);
  }

  public static Selection WithEmptyAction(this string label) {
    return new SelectionAction(label, () => { });
  }
}

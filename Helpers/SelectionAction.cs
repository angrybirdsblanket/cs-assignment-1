namespace PokemonPocket.Helpers;
using System;

public class SelectionAction : Selection {
  
  Action Callback;

  public SelectionAction(string label, Action callback) : base(label) {
    this.Callback = callback;
  }

}

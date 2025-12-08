using SenacStore.Domain.Entities;   // Entidade Categoria
using SenacStore.UI.Navigation;     // ICrudNavigator para navegar/voltar entre UCs

namespace SenacStore.UI.UserControls
{
    // Parte parcial do UserControl (InitializeComponent e elementos visuais estão no Designer)
    public partial class ucCategorias : UserControl
    {
  

        // Construtor: recebe navigator, repositório e opcionalmente o id para editar
        public ucCategorias()
        {
            InitializeComponent(); // Inicializa componentes do Designer (controles visuais)

        }
  
    }
}

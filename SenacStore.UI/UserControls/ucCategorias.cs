using System.Diagnostics.CodeAnalysis;
using SenacStore.Domain.Entities;
using SenacStore.UI.Navigation;   // Entidade Categoria
                                  // ICrudNavigator para navegar/voltar entre UCs

namespace SenacStore.UI.UserControls
{
    // Parte parcial do UserControl (InitializeComponent e elementos visuais estão no Designer)
    public partial class ucCategorias : UserControl
    {

        private readonly ICrudNavigator _nav;          // Navegador para abrir/voltar entre UCs
        private readonly ICategoriaRepository _repo;   // Repositório para operações CRUD de Categoria
        private readonly Guid? _id;         // Id da categoria sendo editada (null se for criação)

        // Construtor: recebe navigator, repositório e opcionalmente o id para editar
        public ucCategorias(ICrudNavigator nav, ICategoriaRepository repo, Guid? id = null)
        {
            InitializeComponent(); // Inicializa componentes do Designer (controles visuais)
            _nav = nav; // armazena navigator
            _repo = repo; // armazena repositório
            _id = id; // armazena id (pode ser null)

            // se id for fornecido, estamos editando uma categoria existente
            if (_id.HasValue)
                CarregarCategoria(_id.Value);
        }

        // Carrega os dados da categoria para edição
        private void CarregarCategoria(Guid id)
        {
            var c = _repo.ObterPorId(id);
            if (c == null) return; // se não encontrou, sai sem alterar a UI

            txtNome.Text = c.Nome; // preenche o TextBox com o nome da categoria
        }


        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                mdMessage.Show("Nome obrigatório!");
                return;
            }

            if (_id.HasValue)
            {
                var c = _repo.ObterPorId(_id.Value);
                c.Nome = txtNome.Text.Trim();
                _repo.Atualizar(c);
            }
            else
            {
                var novo = new Categoria
                {
                    Id = Guid.NewGuid(), //gera novo id
                    Nome = txtNome.Text.Trim() //define nome informado
                };
                _repo.Criar(novo);
            }
            _nav.Voltar(); //volta para o ucCrudPadrao
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _nav.Voltar(); //volta para o ucCrudPadrao
        }
    }
}

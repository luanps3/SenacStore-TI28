using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SenacStore.Domain.Entities;
using SenacStore.Infrastructure.IoC;

namespace SenacStore.UI
{
    public partial class frmMenu : Form
    {
        private readonly Stack<UserControl> _historico = new Stack<UserControl>(); // pilha para histórico de navegação
        private Usuario _usuario;
        public frmMenu(Usuario usuario)
        {
            InitializeComponent();
            guna2BorderlessForm1.ResizeForm = false; // impede redimensionamento do formulário

            _usuario = usuario ?? throw new ArgumentNullException(nameof(usuario)); // garante que o usuário não seja nulo

            lblUsuario.Text = $"Bem vindo, {_usuario.Nome}";

        }

        // Atualiza foto e nome do usuário exibidos no menu lateral
        private void RefreshUserPhoto()
        {
            try
            {
                if (_usuario == null) return; // se não há usuário, nada a fazer

                // obtém repositório via IoC e busca a versão atualizada do usuário no banco
                var repo = IoC.UsuarioRepository();
                var u = repo.ObterPorId(_usuario.Id) ?? _usuario;

                // atualiza a referência local e o texto do label
                _usuario = u;
                lblUsuario.Text = $"Bem-vindo, {_usuario.Nome}";

                // decide o caminho relativo a usar: FotoUrl do usuário ou imagem padrão ("img/user2.png")
                string rel = !string.IsNullOrWhiteSpace(_usuario.FotoUrl)
                    ? _usuario.FotoUrl
                    : Path.Combine("img", "user2.png").Replace('\\', '/');

                // constrói caminho físico absoluto a partir do diretório base da aplicação
                var fisico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, rel.Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(fisico))
                {
                    // carrega a imagem do arquivo físico e atribui ao PictureBox (usa Bitmap para evitar bloqueio)
                    using var imgTemp = Image.FromFile(fisico);
                    pbFoto.Image = new Bitmap(imgTemp);
                }
                else
                {
                    // se o arquivo não existe, tenta usar a imagem embutida nos recursos (fallback)
                    try
                    {
                        pbFoto.Image = Properties.Resources.user2;
                    }
                    catch
                    {
                        pbFoto.Image = null; // se recurso não existir, limpa a imagem
                    }
                }
            }
            catch
            {
                // evita que erro ao carregar imagem quebre a UI — falha silenciosa intencional
            }
        }


        private void frmMenu_Load(object sender, EventArgs e)
        {
            RefreshUserPhoto();
        }

        public void Abrir(UserControl controle)
        {
            panel.Controls.Clear();
            controle.Dock = DockStyle.Fill;
            panel.Controls.Add(controle);

            _historico.Push(controle); // adiciona ao histórico

            if (controle is IRefreshable refreshable)
            {
                // chama RefreshGrid de forma assíncrona para evitar travar a UI
                this.BeginInvoke((Action)(() => refreshable.RefreshGrid())); // invoca no thread da UI
            }
        }
    }
}

# Teste agora — Hitman Contracts 1.2.6

Os códigos entre colchetes são do `CHECKLIST_TESTES.md`. Pode continuar no **mesmo save**. **Feche o jogo uma vez**: a 1.2.6 se instala sozinha nesse momento. Depois abra de novo e confira no log "Hitman Contracts **1.2.6** loaded".

## Novo na 1.2.6 (faça primeiro)
- [ ] **Policial pelo telefone:** ligue para ele. Os 3 pacotes aparecem **e ficam**, no topo da lista?
- [ ] **Fixer pelo telefone:** ligue para ele. Ele diz **onde está** ("Tô em casa / no trabalho / em … (prédio). Vem falar comigo pessoalmente.") e desliga. O lugar aparece na tela?
- [ ] **Fixer pessoalmente:** vá até ele (ou menu: "Ir ao Fixer"). As opções dele aparecem **no topo**?
  - "Tem serviço pra mim?": ele fala o alvo, onde trabalha e onde entregar?
  - O caso abriu sozinho, já aceito? O sinal de 50% caiu?
  - Mate o alvo e entregue: valida?
- [ ] **Loja e ficha** do Fixer pessoalmente funcionam?
- [ ] **Interrogatório e suborno:** as respostas e a opção de suborno também aparecem em primeiro?

## Da 1.2.4
- [ ] **Abertura:** o log mostra "Loaded **37** custom dialogs"?
- [ ] **[P3]** Ligue para o policial. Depois de "O que você quer que suma…", os **3 pacotes** ficam na tela, sem trocar pelas falas comuns?
  - Escolha um: ele dá o orelhão do envelope?
- [ ] **[K3–K5]** Ligue para o Fixer: as opções dele ficam na tela?
- [ ] **[P3b/K3b]** Fale **pessoalmente** com o policial e com o Fixer (menu: "Ir ao policial", "Ir ao Fixer"). As opções aparecem? Com o policial do lado, o pagamento é na hora.
- [ ] **Porta de casa:** menu → "Ir à porta da casa do Fixer (lado de fora)" (seção Carreira e Fixer) e "Ir à porta da casa do policial (lado de fora)" (seção Policial corrupto). Você aparece no corredor ou na rua, na frente da porta deles?
- [ ] Usou "identificar última chamada" no telefone? Se usou, me diga: apareceu um erro desse serviço no log anterior.

## Novo na 1.2.3
- [ ] **[M7]** Os casos das missões do mod têm **[Hitman]** no nome? (Aceite um contrato novo.)
- [ ] **[M8]** Menu Insert → "» Missões Hitman": a janela lista as missões com os detalhes, e os 8 botões funcionam (principalmente "Ir ao bilhete" e "Aceitar já").
- [ ] **[M9]** Menu Insert → "Mercados negros" → "» Lista de mercados negros": aparece a **senha** de cada um. "Ir para a porta (lado de fora)" te deixa na entrada, e a senha funciona na porta?

## 0. Reteste das correções da 1.2.2 (faça primeiro)
- [ ] **Caderno:** os números do policial e do Fixer aparecem como **"Policial Corrupto"** e **"Fixer"** (os que você já tem também, depois de carregar o save)?
- [ ] **[P3]** Ligue para o policial. Ele atende "Fala. O que você quer que suma do arquivo?" e aparecem os **3 pacotes**, cada um com heat e preço:
  1. "Some com os depoimentos das testemunhas" (mais barato);
  2. "Apaga as gravações das câmeras e os depoimentos";
  3. "Tira meu nome da investigação" (heat zerado, mais caro).
  - Aparece algum erro vermelho no BepInEx?
- [ ] **[P4]** Escolha um pacote, deixe o envelope no orelhão e veja o heat cair.
- [ ] **[P3b]** Menu: "Ir ao policial". Fale com ele **pessoalmente**: os 3 pacotes aparecem, e o pagamento é na hora.
- [ ] **[P1]** Com o contato já feito, F4: agora ele só lembra o número ("…me liga no (número) ou me procura").
- [ ] **[K3–K5]** Ligue para o Fixer: "Tem serviço pra mim?", "Como anda a minha fama?" e a loja aparecem e funcionam.
- [ ] **[K3b]** Menu: "Ir ao Fixer". Fale com ele **pessoalmente**: as mesmas opções aparecem?
- [ ] **Brigas:** provoque um NPC comum (não Enforcer, não segurança). Ele briga **na mão**, sem sacar arma? Um Enforcer ainda saca a dele?
- [ ] **Mira:** a letal e o dardo acertam onde você mira já no 1º tiro?
- [ ] **[I1–I4]** Interrogatório (F12 em casa): os Enforcers ficam no corredor e as respostas aparecem?
- [ ] **[L1–L6]** Campanha (fique em casa nas ligações): "Começar", "Sobre o cliente grande…" no Fixer, "Traição agora", "Quero entregar…" no policial (por telefone ou pessoalmente).

## Já passou ✅
- **[A1/A2]** Carregamento.
- **[C6/N5/V6]** Entrega do Distintivo com pagamento e nota.
- **[V2]** Corpo achado: heat e cena de crime.
- **[P1/K2]** O policial e o Fixer falam quando ligam e o número vai para o caderno.
- **[W]** Letal mata no 1º tiro.

## Ainda falta testar
- **[W3]** Dardo de 3 minutos.
- **[C1–C3]** Sinal de 50% no encontro.
- **[S2]** Suborno aceito por um alvo de salário alto.
- **[V3–V5]** Testemunhas.
- **[X1/X3/X4/X5]** Revólver, faca, garrote, veneno.
- **[K1/K6]** Reputação; intimidação.
- **[N1–N4]** Ligação, Orelhão, Passional, Distintivo.
- **[T1–T6]** Consequências.
- **[L7]** Final A, matando o Fixer.
- **[W7/C9/T7/L8]** Salvar e carregar.
- **[D1/D2]** Side jobs comuns.

**No fim:** feche o jogo e me mande os resultados (✅ ❌ ⚠️ por código). O log eu leio direto do PC.

var erc20 = artifacts.require("./StandardERC20.sol");
var erc20Transfer = artifacts.require("./ERC20TransferContract.sol");
module.exports = function(deployer) {
      deployer.deploy(erc20, 1000000).then(() => deployer.deploy(erc20Transfer, erc20.address));
}
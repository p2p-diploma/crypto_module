var transferAgreement = artifacts.require ("./EthereumTransferContract.sol");
module.exports = function(deployer) {
      deployer.deploy(transferAgreement);
}